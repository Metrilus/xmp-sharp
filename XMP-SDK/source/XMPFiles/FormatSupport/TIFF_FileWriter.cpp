// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2006-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "TIFF_Support.hpp"

// =================================================================================================
/// \file TIFF_FileWriter.cpp
/// \brief TIFF_FileWriter is used for memory-based read-write access and all file-based access.
///
/// \c TIFF_FileWriter is used for memory-based read-write access and all file-based access. The
/// main internal data structure is the InternalTagMap, a std::map that uses the tag number as the
/// key and InternalTagInfo as the value. There are 5 of these maps, one for each of the recognized
/// IFDs. The maps contain an entry for each tag in the IFD, whether we capture the data or not. The
/// dataPtr and dataLen fields in the InternalTagInfo are zero if the tag is not captured.
// =================================================================================================

// =================================================================================================
// TIFF_FileWriter::TIFF_FileWriter
// ================================
//
// Set big endian Get/Put routines so that routines are in place for creating TIFF without a parse.
// Parsing will reset them to the proper endianness for the stream. Big endian is a good default
// since JPEG and PSD files are big endian overall.

TIFF_FileWriter::TIFF_FileWriter()
	: changed(false), memParsed(false), fileParsed(false), ownedStream(false), memStream(0), tiffLength(0)
{

	XMP_Uns8 bogusTIFF [kEmptyTIFFLength];
	
	bogusTIFF[0] = 0x4D;
	bogusTIFF[1] = 0x4D;
	bogusTIFF[2] = 0x00;
	bogusTIFF[3] = 0x2A;
	bogusTIFF[4] = bogusTIFF[5] = bogusTIFF[6] = bogusTIFF[7] = 0x00;
	
	(void) this->CheckTIFFHeader ( bogusTIFF, sizeof ( bogusTIFF ) );
	
}	// TIFF_FileWriter::TIFF_FileWriter

// =================================================================================================
// TIFF_FileWriter::~TIFF_FileWriter
// =================================
//
// The InternalTagInfo destructor will deallocate the data for changed tags. It does not know
// whether they are memory-based or file-based though, so it won't deallocate captured but unchanged
// file-based tags. Mark those as changed here to make the destructor deallocate them.

TIFF_FileWriter::~TIFF_FileWriter()
{
	XMP_Assert ( ! (this->memParsed && this->fileParsed) );

	if ( this->fileParsed && (this->jpegTNailPtr != 0) ) free ( this->jpegTNailPtr );
	if ( this->ownedStream ) {
		XMP_Assert ( this->memStream != 0 );
		free ( this->memStream );
	}

	if ( this->fileParsed ) {
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
			InternalTagMap& currTagMap ( this->containedIFDs[ifd].tagMap );
			InternalTagMap::iterator tagPos = currTagMap.begin();
			InternalTagMap::iterator tagEnd = currTagMap.end();
			for ( ; tagPos != tagEnd; ++tagPos ) {
				if ( tagPos->second.dataPtr != 0 ) tagPos->second.changed = true;
			}
		}
	}

}	// TIFF_FileWriter::~TIFF_FileWriter

// =================================================================================================
// TIFF_FileWriter::DeleteExistingInfo
// ===================================

void TIFF_FileWriter::DeleteExistingInfo()
{
	XMP_Assert ( ! (this->memParsed && this->fileParsed) );

	if ( this->ownedStream ) free ( this->memStream );	// ! Current TIFF might be memory-parsed.
	this->memStream = 0;
	this->tiffLength = 0;

	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) this->containedIFDs[ifd].clear();

	this->changed = false;
	this->memParsed = false;
	this->fileParsed = false;
	this->ownedStream = false;

}	// TIFF_FileWriter::DeleteExistingInfo

// =================================================================================================
// TIFF_FileWriter::PickIFD
// ========================

XMP_Uns8 TIFF_FileWriter::PickIFD ( XMP_Uns8 ifd, XMP_Uns16 id )
{
	if ( ifd > kTIFF_LastRealIFD ) {
		if ( ifd != kTIFF_KnownIFD ) XMP_Throw ( "Invalid IFD number", kXMPErr_BadParam );
		XMP_Throw ( "kTIFF_KnownIFD not yet implemented", kXMPErr_Unimplemented );
		// *** Likely to stay unimplemented until there is a client need.
	}

	return ifd;

}	// TIFF_FileWriter::PickIFD

// =================================================================================================
// TIFF_FileWriter::FindTagInIFD
// =============================

const TIFF_FileWriter::InternalTagInfo* TIFF_FileWriter::FindTagInIFD ( XMP_Uns8 ifd, XMP_Uns16 id ) const
{
	ifd = PickIFD ( ifd, id );
	const InternalTagMap& currIFD = this->containedIFDs[ifd].tagMap;

	InternalTagMap::const_iterator tagPos = currIFD.find ( id );
	if ( tagPos == currIFD.end() ) return 0;
	return &tagPos->second;

}	// TIFF_FileWriter::FindTagInIFD

// =================================================================================================
// TIFF_FileWriter::GetIFD
// =======================

bool TIFF_FileWriter::GetIFD ( XMP_Uns8 ifd, TagInfoMap* ifdMap ) const 
{
	if ( ifd > kTIFF_LastRealIFD ) XMP_Throw ( "Invalid IFD number", kXMPErr_BadParam );
	const InternalTagMap& currIFD = this->containedIFDs[ifd].tagMap;

	InternalTagMap::const_iterator tagPos = currIFD.begin();
	InternalTagMap::const_iterator tagEnd = currIFD.end();
	
	if ( ifdMap != 0 ) ifdMap->clear();
	if ( tagPos == tagEnd ) return false;	// Empty IFD.
	
	if ( ifdMap != 0 ) {
		for ( ; tagPos != tagEnd; ++tagPos ) {
			const InternalTagInfo& intInfo = tagPos->second;
			TagInfo extInfo ( intInfo.id, intInfo.type, intInfo.count, intInfo.dataPtr, intInfo.dataLen  );
			(*ifdMap)[intInfo.id] = extInfo;
		}
	}
	
	return true;

}	// TIFF_FileWriter::GetIFD

// =================================================================================================
// TIFF_FileWriter::GetValueOffset
// ===============================

XMP_Uns32 TIFF_FileWriter::GetValueOffset ( XMP_Uns8 ifd, XMP_Uns16 id ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( (thisTag == 0) || (thisTag->origLen == 0) ) return 0;
	
	return thisTag->origOffset;
	
}	// TIFF_FileWriter::GetValueOffset

// =================================================================================================
// TIFF_FileWriter::GetTag
// =======================

bool TIFF_FileWriter::GetTag ( XMP_Uns8 ifd, XMP_Uns16 id, TagInfo* info ) const 
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	
	if ( info != 0 ) {

		info->id = thisTag->id;
		info->type = thisTag->type;
		info->count = thisTag->dataLen / kTIFF_TypeSizes[thisTag->type];
		info->dataLen = thisTag->dataLen;
		info->dataPtr = (const void*)(thisTag->dataPtr);

	}
	
	return true;
	
}	// TIFF_FileWriter::GetTag

// =================================================================================================
// TIFF_FileWriter::SetTag
// =======================

void TIFF_FileWriter::SetTag ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns16 type, XMP_Uns32 count, const void* clientPtr ) 
{
	if ( (type < kTIFF_ByteType) || (type > kTIFF_LastType) ) XMP_Throw ( "Invalid TIFF tag type", kXMPErr_BadParam );
	size_t typeSize = kTIFF_TypeSizes[type];
	size_t fullSize = count * typeSize;
	
	ifd = PickIFD ( ifd, id );
	InternalTagMap& currIFD = this->containedIFDs[ifd].tagMap;

	InternalTagMap::iterator tagPos = currIFD.find ( id );
	if ( (tagPos != currIFD.end()) &&
		 (type == tagPos->second.type) &&
		 (count == tagPos->second.count) &&
		 (memcmp ( clientPtr, tagPos->second.dataPtr, tagPos->second.dataLen ) == 0) ) {
		return;	// ! The value is unchanged, exit.
	}

	InternalTagInfo newTag ( id, type, count );
	newTag.changed = true;
	newTag.dataLen = count * typeSize;
	
	if ( newTag.dataLen <= 4 ) {
		// The data is less than 4 bytes, store it in the dataOrOffset field. Numbers are already flipped.
		XMP_Assert ( sizeof ( newTag.dataOrOffset ) == 4 );
		memcpy ( &newTag.dataOrOffset, clientPtr, newTag.dataLen );	// AUDIT: Safe, the length is <= 4.
	} else {
		// The data is more than 4 bytes, make a copy.
		newTag.dataPtr = (XMP_Uns8*) malloc ( newTag.dataLen );
		if ( newTag.dataPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( newTag.dataPtr, clientPtr, newTag.dataLen );	// AUDIT: Safe, malloc'ed newTag.dataLen bytes above.
	}

	if ( tagPos == currIFD.end() ) {
		currIFD[id] = newTag;
	} else {
		newTag.origLen = tagPos->second.origLen;
		newTag.origOffset = tagPos->second.origOffset;
		tagPos->second = newTag;	// ! The InternalTagInfo assign operator transfers dataPtr ownership.
	}
	
	this->containedIFDs[ifd].changed = true;
	this->changed = true;

}	// TIFF_FileWriter::SetTag

// =================================================================================================
// TIFF_FileWriter::DeleteTag
// ==========================

void TIFF_FileWriter::DeleteTag ( XMP_Uns8 ifd, XMP_Uns16 id ) 
{
	ifd = PickIFD ( ifd, id );
	InternalTagMap& currIFD = this->containedIFDs[ifd].tagMap;
	
	InternalTagMap::iterator tagPos = currIFD.find ( id );
	if ( tagPos == currIFD.end() ) return;	// ! Don't set the changed flags if the tag didn't exist.

	currIFD.erase ( tagPos );
	this->containedIFDs[ifd].changed = true;
	this->changed = true;

}	// TIFF_FileWriter::DeleteTag

// =================================================================================================
// TIFF_FileWriter::GetTag_Integer
// ===============================

bool TIFF_FileWriter::GetTag_Integer ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns32* data ) const 
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	
	if ( data != 0 ) {
		if ( thisTag->type == kTIFF_ShortType ) {
			if ( thisTag->dataLen != 2 ) return false;	// Wrong count.
			*data = this->GetUns16 ( thisTag->dataPtr );
		} else if ( thisTag->type == kTIFF_LongType ) {
			if ( thisTag->dataLen != 4 ) return false;	// Wrong count.
			*data = this->GetUns32 ( thisTag->dataPtr );
		} else {
			return false;
		}
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Integer

// =================================================================================================
// TIFF_FileWriter::GetTag_Byte
// ============================

bool TIFF_FileWriter::GetTag_Byte ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns8* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_ByteType) || (thisTag->dataLen != 1) ) return false;
	
	if ( data != 0 ) *data = *thisTag->dataPtr;
	
	return true;

}	// TIFF_FileWriter::GetTag_Byte

// =================================================================================================
// TIFF_FileWriter::GetTag_SByte
// =============================

bool TIFF_FileWriter::GetTag_SByte ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int8* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SByteType) || (thisTag->dataLen != 1) ) return false;
	
	if ( data != 0 ) *data = *thisTag->dataPtr;
	
	return true;

}	// TIFF_FileWriter::GetTag_SByte

// =================================================================================================
// TIFF_FileWriter::GetTag_Short
// =============================

bool TIFF_FileWriter::GetTag_Short ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns16* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_ShortType) || (thisTag->dataLen != 2) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetUns16 ( thisTag->dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Short

// =================================================================================================
// TIFF_FileWriter::GetTag_SShort
// ==============================

bool TIFF_FileWriter::GetTag_SShort ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int16* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SShortType) || (thisTag->dataLen != 2) ) return false;
	
	if ( data != 0 ) {
		*data = (XMP_Int16) this->GetUns16 ( thisTag->dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_SShort

// =================================================================================================
// TIFF_FileWriter::GetTag_Long
// ============================

bool TIFF_FileWriter::GetTag_Long ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns32* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_LongType) || (thisTag->dataLen != 4) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetUns32 ( thisTag->dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Long

// =================================================================================================
// TIFF_FileWriter::GetTag_SLong
// =============================

bool TIFF_FileWriter::GetTag_SLong ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int32* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SLongType) || (thisTag->dataLen != 4) ) return false;
	
	if ( data != 0 ) {
		*data = (XMP_Int32) this->GetUns32 ( thisTag->dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_SLong

// =================================================================================================
// TIFF_FileWriter::GetTag_Rational
// ================================

bool TIFF_FileWriter::GetTag_Rational ( XMP_Uns8 ifd, XMP_Uns16 id, Rational* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( (thisTag == 0) || (thisTag->dataPtr == 0) ) return false;
	if ( (thisTag->type != kTIFF_RationalType) || (thisTag->dataLen != 8) ) return false;
	
	if ( data != 0 ) {
		XMP_Uns32* dataPtr = (XMP_Uns32*)thisTag->dataPtr;
		data->num = this->GetUns32 ( dataPtr );
		data->denom = this->GetUns32 ( dataPtr+1 );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Rational

// =================================================================================================
// TIFF_FileWriter::GetTag_SRational
// =================================

bool TIFF_FileWriter::GetTag_SRational ( XMP_Uns8 ifd, XMP_Uns16 id, SRational* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( (thisTag == 0) || (thisTag->dataPtr == 0) ) return false;
	if ( (thisTag->type != kTIFF_SRationalType) || (thisTag->dataLen != 8) ) return false;
	
	if ( data != 0 ) {
		XMP_Uns32* dataPtr = (XMP_Uns32*)thisTag->dataPtr;
		data->num = (XMP_Int32) this->GetUns32 ( dataPtr );
		data->denom = (XMP_Int32) this->GetUns32 ( dataPtr+1 );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_SRational

// =================================================================================================
// TIFF_FileWriter::GetTag_Float
// =============================

bool TIFF_FileWriter::GetTag_Float ( XMP_Uns8 ifd, XMP_Uns16 id, float* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_FloatType) || (thisTag->dataLen != 4) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetFloat ( thisTag->dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Float

// =================================================================================================
// TIFF_FileWriter::GetTag_Double
// ==============================

bool TIFF_FileWriter::GetTag_Double ( XMP_Uns8 ifd, XMP_Uns16 id, double* data ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( (thisTag == 0) || (thisTag->dataPtr == 0) ) return false;
	if ( (thisTag->type != kTIFF_DoubleType) || (thisTag->dataLen != 8) ) return false;
	
	if ( data != 0 ) {
		double* dataPtr = (double*)thisTag->dataPtr;
		*data = this->GetDouble ( dataPtr );
	}
	
	return true;

}	// TIFF_FileWriter::GetTag_Double

// =================================================================================================
// TIFF_FileWriter::GetTag_ASCII
// =============================

bool TIFF_FileWriter::GetTag_ASCII ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_StringPtr* dataPtr, XMP_StringLen* dataLen ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->dataLen > 4) && (thisTag->dataPtr == 0) ) return false;
	if ( thisTag->type != kTIFF_ASCIIType ) return false;
	
	if ( dataPtr != 0 ) *dataPtr = (XMP_StringPtr)thisTag->dataPtr;
	if ( dataLen != 0 ) *dataLen = thisTag->dataLen;
	
	return true;

}	// TIFF_FileWriter::GetTag_ASCII

// =================================================================================================
// TIFF_FileWriter::GetTag_EncodedString
// =====================================

bool TIFF_FileWriter::GetTag_EncodedString ( XMP_Uns8 ifd, XMP_Uns16 id, std::string* utf8Str ) const
{
	const InternalTagInfo* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( thisTag->type != kTIFF_UndefinedType ) return false;
	
	if ( utf8Str == 0 ) return true;	// Return true if the converted string is not wanted.
	
	bool ok = this->DecodeString ( thisTag->dataPtr, thisTag->dataLen, utf8Str );
	return ok;

}	// TIFF_FileWriter::GetTag_EncodedString

// =================================================================================================
// TIFF_FileWriter::SetTag_EncodedString
// =====================================

void TIFF_FileWriter::SetTag_EncodedString ( XMP_Uns8 ifd, XMP_Uns16 id, const std::string& utf8Str, XMP_Uns8 encoding )
{

	XMP_Throw ( "Not yet implemented", kXMPErr_Unimplemented );

}	// TIFF_FileWriter::SetTag_EncodedString

// =================================================================================================
// TIFF_FileWriter::IsLegacyChanged
// ================================

bool TIFF_FileWriter::IsLegacyChanged()
{

	if ( ! this->changed ) return false;
	
	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {

		InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
		if ( ! thisIFD.changed ) continue;
			
		InternalTagMap::iterator tagPos;
		InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
		
		for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
			InternalTagInfo & thisTag = tagPos->second;
			if ( thisTag.changed && (thisTag.id != kTIFF_XMP) ) return true;
		}

	}
	
	return false;	// Can get here if the XMP tag is the only one changed.

}	// TIFF_FileWriter::IsLegacyChanged

// =================================================================================================
// TIFF_FileWriter::ParseMemoryStream
// ==================================

void TIFF_FileWriter::ParseMemoryStream ( const void* data, XMP_Uns32 length, bool copyData /* = true */ ) 
{
	this->DeleteExistingInfo();
	this->memParsed = true;
	if ( length == 0 ) return;

	// Allocate space for the full in-memory stream and copy it.
	
	if ( ! copyData ) {
		XMP_Assert ( ! this->ownedStream );
		this->memStream = (XMP_Uns8*) data;
	} else {
		if ( length > 100*1024*1024 ) XMP_Throw ( "Outrageous length for memory-based TIFF", kXMPErr_BadTIFF );
		this->memStream = (XMP_Uns8*) malloc(length);
		if ( this->memStream == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( this->memStream, data, length );	// AUDIT: Safe, malloc'ed length bytes above.
	}
	this->tiffLength = length;

	// Find and process the primary, Exif, GPS, and Interoperability IFDs.
	
	XMP_Uns32 primaryIFDOffset = this->CheckTIFFHeader ( this->memStream, length );
	XMP_Uns32 tnailIFDOffset   = 0;
	
	if ( primaryIFDOffset != 0 ) tnailIFDOffset = this->ProcessMemoryIFD ( primaryIFDOffset, kTIFF_PrimaryIFD );

	const InternalTagInfo* exifIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer );
	if ( (exifIFDTag != 0) && (exifIFDTag->type == kTIFF_LongType) && (exifIFDTag->dataLen == 4) ) {
		XMP_Uns32 exifOffset = this->GetUns32 ( &exifIFDTag->dataOrOffset );
		(void) this->ProcessMemoryIFD ( exifOffset, kTIFF_ExifIFD );
	}

	const InternalTagInfo* gpsIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer );
	if ( (gpsIFDTag != 0) && (gpsIFDTag->type == kTIFF_LongType) && (gpsIFDTag->dataLen == 4) ) {
		XMP_Uns32 gpsOffset = this->GetUns32 ( &gpsIFDTag->dataOrOffset );
		(void) this->ProcessMemoryIFD ( gpsOffset, kTIFF_GPSInfoIFD );
	}

	const InternalTagInfo* interopIFDTag = this->FindTagInIFD ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer );
	if ( (interopIFDTag != 0) && (interopIFDTag->type == kTIFF_LongType) && (interopIFDTag->dataLen == 4) ) {
		XMP_Uns32 interopOffset = this->GetUns32 ( &interopIFDTag->dataOrOffset );
		(void) this->ProcessMemoryIFD ( interopOffset, kTIFF_InteropIFD );
	}
	
	// Process the thumbnail IFD. We only do this for Exif-compliant TIFF streams. Extract the
	// JPEG thumbnail image pointer (tag 513) for later use by GetTNailInfo.

	if ( (tnailIFDOffset != 0) && (! this->containedIFDs[kTIFF_ExifIFD].tagMap.empty()) ) {
		(void) this->ProcessMemoryIFD ( tnailIFDOffset, kTIFF_TNailIFD );
		const InternalTagInfo* jpegInfo = FindTagInIFD ( kTIFF_TNailIFD, kTIFF_JPEGInterchangeFormat );
		if ( jpegInfo != 0 ) {
			XMP_Uns32 tnailImageOffset = this->GetUns32 ( &jpegInfo->dataOrOffset );
			this->jpegTNailPtr = (XMP_Uns8*)this->memStream + tnailImageOffset;
		}
	}
	
	#if 0
	{
		printf ( "\nExiting TIFF_FileWriter::ParseMemoryStream\n" );
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
			InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
			printf ( "\n   IFD %d, count %d, mapped %d, offset %d (0x%X), next IFD %d (0x%X)\n",
					 ifd, thisIFD.origCount, thisIFD.tagMap.size(),
					 thisIFD.origOffset, thisIFD.origOffset, thisIFD.origNextIFD, thisIFD.origNextIFD );
			InternalTagMap::iterator tagPos;
			InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
			for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
				InternalTagInfo & thisTag = tagPos->second;
				printf ( "      Tag %d, dataOrOffset 0x%X, origLen %d, origOffset %d (0x%X)\n",
						 thisTag.id, thisTag.dataOrOffset, thisTag.origLen, thisTag.origOffset, thisTag.origOffset );
			}
		}
		printf ( "\n" );
	}
	#endif

}	// TIFF_FileWriter::ParseMemoryStream

// =================================================================================================
// TIFF_FileWriter::ProcessMemoryIFD
// =================================

XMP_Uns32 TIFF_FileWriter::ProcessMemoryIFD ( XMP_Uns32 ifdOffset, XMP_Uns8 ifd )
{
	InternalIFDInfo& ifdInfo ( this->containedIFDs[ifd] );
	
	if ( (ifdOffset < 8) || (ifdOffset > (this->tiffLength - kEmptyIFDLength)) ) {
		XMP_Throw ( "Bad IFD offset", kXMPErr_BadTIFF );
	}
		
	XMP_Uns8* ifdPtr = this->memStream + ifdOffset;
	XMP_Uns16 tagCount = this->GetUns16 ( ifdPtr );
	RawIFDEntry* ifdEntries = (RawIFDEntry*)(ifdPtr+2);

	if ( tagCount >= 0x8000 ) XMP_Throw ( "Outrageous IFD count", kXMPErr_BadTIFF );
	if ( (ifdOffset + 2 + tagCount*12 + 4) > this->tiffLength ) XMP_Throw ( "Out of bounds IFD", kXMPErr_BadTIFF );
	
	ifdInfo.origOffset = ifdOffset;
	ifdInfo.origCount  = tagCount;
	
	for ( size_t i = 0; i < tagCount; ++i ) {
	
		RawIFDEntry*    rawTag = &ifdEntries[i];
		InternalTagInfo mapTag ( this->GetUns16(&rawTag->id), this->GetUns16(&rawTag->type), this->GetUns32(&rawTag->count) );
		if ( (mapTag.type < kTIFF_ByteType) || (mapTag.type > kTIFF_LastType) ) continue;	// Bad type, skip this tag.

		mapTag.dataLen = mapTag.origLen = mapTag.count * kTIFF_TypeSizes[mapTag.type];
		mapTag.dataOrOffset = rawTag->dataOrOffset;	// Keep the value or offset in stream byte ordering.

		if ( mapTag.dataLen <= 4 ) {
			mapTag.dataPtr = (XMP_Uns8*) &mapTag.dataOrOffset;
			mapTag.origOffset = ifdOffset + 2 + (12 * i);	// Compute the data offset.
		} else {
			mapTag.origOffset = this->GetUns32 ( &rawTag->dataOrOffset );	// Extract the data offset.
			mapTag.dataPtr = this->memStream + mapTag.origOffset;
			// printf ( "FW_ProcessMemoryIFD tag %d large value @ %.8X\n", mapTag.id, mapTag.dataPtr );
		}
		ifdInfo.tagMap[mapTag.id] = mapTag;
	
	}
	
	ifdPtr += (2 + tagCount*12);
	ifdInfo.origNextIFD = this->GetUns32 ( ifdPtr );
	
	return ifdInfo.origNextIFD;

}	// TIFF_FileWriter::ProcessMemoryIFD

// =================================================================================================
// CaptureJPEGTNail
// ================
//
// Capture the JPEG image stream for an Exif compressed thumbnail.

static XMP_Uns8* CaptureJPEGTNail ( LFA_FileRef fileRef, IOBuffer* ioBuf, const TIFF_Manager& tiff )
{
	bool ok;
	XMP_Uns8* jpegPtr = 0;
	XMP_Uns32 jpegOffset, jpegLen;
	
	ok = tiff.GetTag_Integer ( kTIFF_TNailIFD, kTIFF_JPEGInterchangeFormat, &jpegOffset );
	if ( ok ) ok = tiff.GetTag_Integer ( kTIFF_TNailIFD, kTIFF_JPEGInterchangeFormatLength, &jpegLen );
	if ( ! ok ) return 0;

	if ( jpegLen > 1024*1024 ) return 0;	// ? XMP_Throw ( "Outrageous JPEG TNail length", kXMPErr_BadTIFF );

	jpegPtr = (XMP_Uns8*) malloc ( jpegLen );
	if ( jpegPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
	
	try {
	
		if ( jpegLen > kIOBufferSize ) {
			// This value is bigger than the I/O buffer, read it directly and restore the file position.
			LFA_Seek ( fileRef, jpegOffset, SEEK_SET );
			LFA_Read ( fileRef, jpegPtr, jpegLen, kLFA_RequireAll );
			LFA_Seek ( fileRef, (ioBuf->filePos + ioBuf->len), SEEK_SET );
		} else {
			// This value can fit in the I/O buffer, so use that.
			MoveToOffset ( fileRef, jpegOffset, ioBuf );
			ok = CheckFileSpace ( fileRef, ioBuf, jpegLen );
			if ( ! ok ) XMP_Throw ( "EOF in data block", kXMPErr_BadTIFF );
			memcpy ( jpegPtr, ioBuf->ptr, jpegLen );	// AUDIT: Safe, malloc'ed jpegLen bytes above.
		}
	
	} catch ( ... ) {
	
		free ( jpegPtr );
		throw;
	
	}
	
	return jpegPtr;

}	// CaptureJPEGTNail

// =================================================================================================
// TIFF_FileWriter::ParseFileStream
// ================================
//
// The buffered I/O model is worth the logic complexity - as opposed to a simple seek/read for each
// part of the TIFF stream. The vast majority of real-world TIFFs have the primary IFD, Exif IFD,
// and all of their interesting tag values within the first 64K of the file. Well, at least before
// we get around to our edit-by-append approach.

void TIFF_FileWriter::ParseFileStream ( LFA_FileRef fileRef ) 
{
	bool ok;
	IOBuffer  ioBuf;

	this->DeleteExistingInfo();
	this->fileParsed = true;
	this->tiffLength = (XMP_Uns32) LFA_Measure ( fileRef );
	if ( this->tiffLength == 0 ) return;
	
	// Find and process the primary, Exif, GPS, and Interoperability IFDs.
	
	ioBuf.filePos = LFA_Seek ( fileRef, 0, SEEK_SET );
	ok = CheckFileSpace ( fileRef, &ioBuf, 8 );
	if ( ! ok ) XMP_Throw ( "TIFF too small", kXMPErr_BadTIFF );
		
	XMP_Uns32 primaryIFDOffset = this->CheckTIFFHeader ( ioBuf.ptr, this->tiffLength );
	XMP_Uns32 tnailIFDOffset   = 0;
	
	if ( primaryIFDOffset != 0 ) tnailIFDOffset = this->ProcessFileIFD ( kTIFF_PrimaryIFD, primaryIFDOffset, fileRef, &ioBuf );

	const InternalTagInfo* exifIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer );
	if ( (exifIFDTag != 0) && (exifIFDTag->type == kTIFF_LongType) && (exifIFDTag->count == 1) ) {
		XMP_Uns32 exifOffset = this->GetUns32 ( &exifIFDTag->dataOrOffset );
		(void) this->ProcessFileIFD ( kTIFF_ExifIFD, exifOffset, fileRef, &ioBuf );
	}

	const InternalTagInfo* gpsIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer );
	if ( (gpsIFDTag != 0) && (gpsIFDTag->type == kTIFF_LongType) && (gpsIFDTag->count == 1) ) {
		XMP_Uns32 gpsOffset = this->GetUns32 ( &gpsIFDTag->dataOrOffset );
		(void) this->ProcessFileIFD ( kTIFF_GPSInfoIFD, gpsOffset, fileRef, &ioBuf );
	}

	const InternalTagInfo* interopIFDTag = this->FindTagInIFD ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer );
	if ( (interopIFDTag != 0) && (interopIFDTag->type == kTIFF_LongType) && (interopIFDTag->dataLen == 4) ) {
		XMP_Uns32 interopOffset = this->GetUns32 ( &interopIFDTag->dataOrOffset );
		(void) this->ProcessFileIFD ( kTIFF_InteropIFD, interopOffset, fileRef, &ioBuf );
	}
	
	// Process the thumbnail IFD. We only do this for Exif-compliant TIFF streams. Do this after
	// the others since they are often within the first 64K of the file and the thumbnail is not.

	if ( (tnailIFDOffset != 0) && (! this->containedIFDs[kTIFF_ExifIFD].tagMap.empty()) ) {
		(void) this->ProcessFileIFD ( kTIFF_TNailIFD, tnailIFDOffset, fileRef, &ioBuf );
		this->jpegTNailPtr = CaptureJPEGTNail ( fileRef, &ioBuf, *this );
	}
	
	#if 0
	{
		printf ( "\nExiting TIFF_FileWriter::ParseFileStream\n" );
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
			InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
			printf ( "\n   IFD %d, count %d, mapped %d, offset %d (0x%X), next IFD %d (0x%X)\n",
					 ifd, thisIFD.origCount, thisIFD.tagMap.size(),
					 thisIFD.origOffset, thisIFD.origOffset, thisIFD.origNextIFD, thisIFD.origNextIFD );
			InternalTagMap::iterator tagPos;
			InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
			for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
				InternalTagInfo & thisTag = tagPos->second;
				printf ( "      Tag %d, dataOrOffset 0x%X, origLen %d, origOffset %d (0x%X)\n",
						 thisTag.id, thisTag.dataOrOffset, thisTag.origLen, thisTag.origOffset, thisTag.origOffset );
			}
		}
		printf ( "\n" );
	}
	#endif

}	// TIFF_FileWriter::ParseFileStream

// =================================================================================================
// TIFF_FileWriter::ProcessFileIFD
// ===============================

XMP_Uns32 TIFF_FileWriter::ProcessFileIFD ( XMP_Uns8 ifd, XMP_Uns32 ifdOffset, LFA_FileRef fileRef, IOBuffer* ioBuf ) 
{
	InternalIFDInfo& ifdInfo ( this->containedIFDs[ifd] );
	
	MoveToOffset ( fileRef, ifdOffset, ioBuf );	// Move to the start of the IFD.
	
	bool ok = CheckFileSpace ( fileRef, ioBuf, 2 );
	if ( ! ok ) XMP_Throw ( "IFD count missing", kXMPErr_BadTIFF );
	XMP_Uns16 tagCount = this->GetUns16 ( ioBuf->ptr );

	if ( tagCount >= 0x8000 ) XMP_Throw ( "Outrageous IFD count", kXMPErr_BadTIFF );
	if ( (ifdOffset + 2 + tagCount*12 + 4) > this->tiffLength ) XMP_Throw ( "Out of bounds IFD", kXMPErr_BadTIFF );
	
	ifdInfo.origOffset = ifdOffset;
	ifdInfo.origCount  = tagCount;
	
	// ---------------------------------------------------------------------------------------------
	// First create all of the IFD map entries, capturing short values, and get the next IFD offset.
	// We're using a std::map for storage, it automatically eliminates duplicates and provides
	// sorted output. Plus the "map[key] = value" assignment conveniently keeps the last encountered
	// value, following Photoshop's behavior.

	ioBuf->ptr += 2;	// Move to the first IFD entry.
	
	for ( XMP_Uns16 i = 0; i < tagCount; ++i, ioBuf->ptr += 12 ) {
	
		if ( ! CheckFileSpace ( fileRef, ioBuf, 12 ) ) XMP_Throw ( "EOF within IFD", kXMPErr_BadTIFF );
		
		RawIFDEntry*    rawTag = (RawIFDEntry*)ioBuf->ptr;
		InternalTagInfo mapTag ( this->GetUns16(&rawTag->id), this->GetUns16(&rawTag->type), this->GetUns32(&rawTag->count) );
		if ( (mapTag.type < kTIFF_ByteType) || (mapTag.type > kTIFF_LastType) ) continue;	// Bad type, skip this tag.

		mapTag.dataLen = mapTag.origLen = mapTag.count * kTIFF_TypeSizes[mapTag.type];
		mapTag.dataOrOffset = rawTag->dataOrOffset;	// Keep the value or offset in stream byte ordering.

		if ( mapTag.dataLen <= 4 ) {
			mapTag.dataPtr = (XMP_Uns8*) &mapTag.dataOrOffset;
			mapTag.origOffset = ifdOffset + 2 + (12 * i);	// Compute the data offset.
		} else {
			mapTag.origOffset = this->GetUns32 ( &rawTag->dataOrOffset );	// Extract the data offset.
		}
		ifdInfo.tagMap[mapTag.id] = mapTag;
	
	}
	
	if ( ! CheckFileSpace ( fileRef, ioBuf, 4 ) ) XMP_Throw ( "EOF at next IFD offset", kXMPErr_BadTIFF );
	ifdInfo.origNextIFD = this->GetUns32 ( ioBuf->ptr );
	
	// ---------------------------------------------------------------------------------------------
	// Go back over the tag map and extract the data for large recognized tags. This is done in 2
	// passes, in order to lessen the typical amount of I/O. On the first pass make sure we have at
	// least 32K of data following the IFD in the buffer, and extract all of the values in that
	// portion. This should cover an original file, or the appended values with an appended IFD.
	
	if ( (ioBuf->limit - ioBuf->ptr) < 32*1024 ) RefillBuffer ( fileRef, ioBuf );
	
	InternalTagMap::iterator tagPos = ifdInfo.tagMap.begin();
	InternalTagMap::iterator tagEnd = ifdInfo.tagMap.end();
	
	const XMP_Uns16* knownTagPtr = sKnownTags[ifd];	// Points into the ordered recognized tag list.
	
	XMP_Uns32 bufBegin = (XMP_Uns32)ioBuf->filePos;	// TIFF stream bounds for the current buffer.
	XMP_Uns32 bufEnd   = bufBegin + ioBuf->len;
	
	for ( ; tagPos != tagEnd; ++tagPos ) {
	
		InternalTagInfo* currTag = &tagPos->second;

		if ( currTag->dataLen <= 4 ) continue;	// Short values are already in the dataOrOffset field.
		while ( *knownTagPtr < currTag->id ) ++knownTagPtr;
		if ( *knownTagPtr != currTag->id ) continue;	// Skip unrecognized tags.
		if ( currTag->dataLen > 1024*1024 ) XMP_Throw ( "Outrageous data length", kXMPErr_BadTIFF );
		
		if ( (bufBegin <= currTag->origOffset) && ((currTag->origOffset + currTag->dataLen) <= bufEnd) ) {
			// This value is already fully within the current I/O buffer, copy it.
			MoveToOffset ( fileRef, currTag->origOffset, ioBuf );
			currTag->dataPtr = (XMP_Uns8*) malloc ( currTag->dataLen );
			if ( currTag->dataPtr == 0 ) XMP_Throw ( "No data block", kXMPErr_NoMemory );
			memcpy ( currTag->dataPtr, ioBuf->ptr, currTag->dataLen );	// AUDIT: Safe, malloc'ed currTag->dataLen bytes above.
		}
	
	}
	
	// ---------------------------------------------------------------------------------------------
	// Now the second large value pass. This will reposition the I/O buffer as necessary. Hopefully
	// just once, to pick up the span of data not covered in the first pass.
	
	tagPos = ifdInfo.tagMap.begin();	// Reset both map/array positions.
	knownTagPtr = sKnownTags[ifd];
	
	for ( ; tagPos != tagEnd; ++tagPos ) {
	
		InternalTagInfo* currTag = &tagPos->second;

		if ( (currTag->dataLen <= 4) || (currTag->dataPtr != 0) ) continue;	// Done this tag?
		while ( *knownTagPtr < currTag->id ) ++knownTagPtr;
		if ( *knownTagPtr != currTag->id ) continue;	// Skip unrecognized tags.
		if ( currTag->dataLen > 1024*1024 ) XMP_Throw ( "Outrageous data length", kXMPErr_BadTIFF );

		currTag->dataPtr = (XMP_Uns8*) malloc ( currTag->dataLen );
		if ( currTag->dataPtr == 0 ) XMP_Throw ( "No data block", kXMPErr_NoMemory );
		
		if ( currTag->dataLen > kIOBufferSize ) {
			// This value is bigger than the I/O buffer, read it directly and restore the file position.
			LFA_Seek ( fileRef, currTag->origOffset, SEEK_SET );
			LFA_Read ( fileRef, currTag->dataPtr, currTag->dataLen, kLFA_RequireAll );
			LFA_Seek ( fileRef, (ioBuf->filePos + ioBuf->len), SEEK_SET );
		} else {
			// This value can fit in the I/O buffer, so use that.
			MoveToOffset ( fileRef, currTag->origOffset, ioBuf );
			ok = CheckFileSpace ( fileRef, ioBuf, currTag->dataLen );
			if ( ! ok ) XMP_Throw ( "EOF in data block", kXMPErr_BadTIFF );
			memcpy ( currTag->dataPtr, ioBuf->ptr, currTag->dataLen );	// AUDIT: Safe, malloc'ed currTag->dataLen bytes above.
		}
		
	}
	
	// Done, return the next IFD offset.
	
	return ifdInfo.origNextIFD;

}	// TIFF_FileWriter::ProcessFileIFD

// =================================================================================================
// TIFF_FileWriter::IntegrateFromPShop6
// ====================================
//
// See comments for ProcessPShop6IFD.

void TIFF_FileWriter::IntegrateFromPShop6 ( const void * buriedPtr, size_t buriedLen ) 
{
	TIFF_MemoryReader buriedExif;
	buriedExif.ParseMemoryStream ( buriedPtr, buriedLen );
	
	this->ProcessPShop6IFD ( buriedExif, kTIFF_PrimaryIFD );
	this->ProcessPShop6IFD ( buriedExif, kTIFF_TNailIFD );
	this->ProcessPShop6IFD ( buriedExif, kTIFF_ExifIFD );
	this->ProcessPShop6IFD ( buriedExif, kTIFF_GPSInfoIFD );

}	// TIFF_FileWriter::IntegrateFromPShop6

// =================================================================================================
// TIFF_FileWriter::CopyTagToMasterIFD
// ===================================
//
// Create a new master IFD entry from a buried Photoshop 6 IFD entry. Don't try to get clever with
// large values, just create a new copy. This preserves a clean separation between the memory-based
// and file-based TIFF processing.

void* TIFF_FileWriter::CopyTagToMasterIFD ( const TagInfo & ps6Tag, InternalIFDInfo * masterIFD )
{
	InternalTagInfo newTag ( ps6Tag.id, ps6Tag.type, ps6Tag.count );

	newTag.dataLen = ps6Tag.dataLen;
	
	if ( newTag.dataLen <= 4 ) {
		newTag.dataPtr = (XMP_Uns8*) &newTag.dataOrOffset;
		newTag.dataOrOffset = *((XMP_Uns32*)ps6Tag.dataPtr);
	} else {
		XMP_Assert ( newTag.dataOrOffset == 0 );
		newTag.dataPtr = (XMP_Uns8*) malloc ( newTag.dataLen );
		if ( newTag.dataPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( newTag.dataPtr, ps6Tag.dataPtr, newTag.dataLen );	// AUDIT: Safe, malloc'ed dataLen bytes above.
	}

	newTag.changed = true;	// ! See comments with ProcessPShop6IFD.
	XMP_Assert ( (newTag.origLen == 0) && (newTag.origOffset == 0) );
	
	masterIFD->tagMap[newTag.id] = newTag;
	masterIFD->changed = true;
	
	return masterIFD->tagMap[newTag.id].dataPtr;	// ! Return the address within the map entry for small values.

}	// TIFF_FileWriter::CopyTagToMasterIFD

// =================================================================================================
// FlipCFATable
// ============
//
// The CFA pattern table is trivial, a pair of short counts followed by n*m bytes.

static bool FlipCFATable ( void* voidPtr, XMP_Uns32 tagLen, GetUns16_Proc GetUns16 )
{
	if ( tagLen < 4 ) return false;
	
	XMP_Uns16* u16Ptr = (XMP_Uns16*)voidPtr;

	Flip2 ( &u16Ptr[0] );	// Flip the counts to match the master TIFF.
	Flip2 ( &u16Ptr[1] );
	
	XMP_Uns16 columns = GetUns16 ( &u16Ptr[0] );	// Fetch using the master TIFF's routine.
	XMP_Uns16 rows    = GetUns16 ( &u16Ptr[1] );
	
	if ( tagLen != (XMP_Uns32)(4 + columns*rows) ) return false;
	
	return true;

}	// FlipCFATable

// =================================================================================================
// FlipDSDTable
// ============
//
// The device settings description table is trivial, a pair of short counts followed by UTF-16
// strings. So the whole value should be flipped as a sequence of 16 bit items.

// ! The Exif 2.2 description is a bit garbled. It might be wrong. It would be nice to have a real example.

static bool FlipDSDTable ( void* voidPtr, XMP_Uns32 tagLen, GetUns16_Proc GetUns16 )
{
	if ( tagLen < 4 ) return false;
	
	XMP_Uns16* u16Ptr = (XMP_Uns16*)voidPtr;
	for ( size_t i = tagLen/2; i > 0; --i, ++u16Ptr ) Flip2 ( u16Ptr );
	
	return true;
	
}	// FlipDSDTable

// =================================================================================================
// FlipOECFSFRTable
// ================
//
// The OECF and SFR tables have the same layout:
//    2 short counts, columns and rows
//    c ASCII strings, null terminated, column names
//    c*r rationals

static bool FlipOECFSFRTable ( void* voidPtr, XMP_Uns32 tagLen, GetUns16_Proc GetUns16 )
{
	XMP_Uns16* u16Ptr = (XMP_Uns16*)voidPtr;

	Flip2 ( &u16Ptr[0] );	// Flip the data to match the master TIFF.
	Flip2 ( &u16Ptr[1] );
	
	XMP_Uns16 columns = GetUns16 ( &u16Ptr[0] );	// Fetch using the master TIFF's routine.
	XMP_Uns16 rows    = GetUns16 ( &u16Ptr[1] );
	
	XMP_Uns32 minLen = 4 + columns + (8 * columns * rows);	// Minimum legit tag size.
	if ( tagLen < minLen ) return false;
	
	// Compute the start of the rationals from the end of value. No need to walk through the names.
	XMP_Uns32* u32Ptr = (XMP_Uns32*) ((XMP_Uns8*)voidPtr + tagLen - (8 * columns * rows));

	for ( size_t i = 2*columns*rows; i > 0; --i, ++u32Ptr ) Flip4 ( u32Ptr );
	
	return true;
	
}	// FlipOECFSFRTable

// =================================================================================================
// TIFF_FileWriter::ProcessPShop6IFD
// =================================
//
// Photoshop 6 wrote wacky TIFF files that have much of the Exif metadata buried inside of image
// resource 1058, which is itself within tag 34377 in the 0th IFD. This routine moves the buried
// tags up to the parent file. Existing tags are not replaced.
//
// While it is tempting to try to directly use the TIFF_MemoryReader's tweaked IFD info, making that
// visible would compromise implementation separation. Better to pay the modest runtime cost of 
// using the official GetIFD method, letting it build the map.
//
// The tags that get moved are marked as being changed, as is the IFD they are moved into, but the
// overall TIFF_FileWriter object is not. We don't want this integration on its own to force a file
// update, but a file update should include these changes.

// ! Be careful to not move tags that are the nasty Exif explicit offsets, e.g. the Exif or GPS IFD
// ! "pointers". These are tags with a LONG type and count of 1, whose value is an offset into the
// ! buried TIFF stream. We can't reliably plant that offset into the outer IFD structure.

// ! To make things even more fun, the buried Exif might not have the same endianness as the outer!

void TIFF_FileWriter::ProcessPShop6IFD ( const TIFF_MemoryReader& buriedExif, XMP_Uns8 ifd )
{
	bool ok, found;
	TagInfoMap ps6IFD;
	
	found = buriedExif.GetIFD ( ifd, &ps6IFD );
	if ( ! found ) return;
	
	bool needsFlipping = (this->bigEndian != buriedExif.IsBigEndian());
	
	InternalIFDInfo* masterIFD = &this->containedIFDs[ifd];
	
	TagInfoMap::const_iterator ps6Pos = ps6IFD.begin();
	TagInfoMap::const_iterator ps6End = ps6IFD.end();
		
	for ( ; ps6Pos != ps6End; ++ps6Pos ) {
	
		// Copy buried tags to the master IFD if they don't already exist there.
		
		const TagInfo& ps6Tag = ps6Pos->second;
	
		if ( this->FindTagInIFD ( ifd, ps6Tag.id ) != 0 ) continue;	// Keep existing master tags.
		if ( needsFlipping && (ps6Tag.id == 37500) ) continue;	// Don't copy an unflipped MakerNote.
		if ( (ps6Tag.id == kTIFF_ExifIFDPointer) ||	// Skip the tags that are explicit offsets.
			 (ps6Tag.id == kTIFF_GPSInfoIFDPointer) ||
			 (ps6Tag.id == kTIFF_JPEGInterchangeFormat) ||
			 (ps6Tag.id == kTIFF_InteroperabilityIFDPointer) ) continue;
		
		void* voidPtr = CopyTagToMasterIFD ( ps6Tag, masterIFD );
		
		if ( needsFlipping ) {
			switch ( ps6Tag.type ) {
	
				case kTIFF_ByteType:
				case kTIFF_SByteType:
				case kTIFF_ASCIIType:
					// Nothing more to do.
					break;
	
				case kTIFF_ShortType:
				case kTIFF_SShortType:
					{
						XMP_Uns16* u16Ptr = (XMP_Uns16*)voidPtr;
						for ( size_t i = ps6Tag.count; i > 0; --i, ++u16Ptr ) Flip2 ( u16Ptr );
					}
					break;
	
				case kTIFF_LongType:
				case kTIFF_SLongType:
				case kTIFF_FloatType:
					{
						XMP_Uns32* u32Ptr = (XMP_Uns32*)voidPtr;
						for ( size_t i = ps6Tag.count; i > 0; --i, ++u32Ptr ) Flip4 ( u32Ptr );
					}
					break;
	
				case kTIFF_RationalType:
				case kTIFF_SRationalType:
					{
						XMP_Uns32* ratPtr = (XMP_Uns32*)voidPtr;
						for ( size_t i = (2 * ps6Tag.count); i > 0; --i, ++ratPtr ) Flip4 ( ratPtr );
					}
					break;
	
				case kTIFF_DoubleType:
					{
						XMP_Uns64* u64Ptr = (XMP_Uns64*)voidPtr;
						for ( size_t i = ps6Tag.count; i > 0; --i, ++u64Ptr ) Flip8 ( u64Ptr );
					}
					break;
	
				case kTIFF_UndefinedType:
					// Fix up the few kinds of special tables that Exif 2.2 defines.
					ok = true;	// Keep everything that isn't a special table.
					if ( ps6Tag.id == kTIFF_CFAPattern ) {
						ok = FlipCFATable ( voidPtr, ps6Tag.dataLen, this->GetUns16 );
					} else if ( ps6Tag.id == kTIFF_DeviceSettingDescription ) {
						ok = FlipDSDTable ( voidPtr, ps6Tag.dataLen, this->GetUns16 );
					} else if ( (ps6Tag.id == kTIFF_OECF) || (ps6Tag.id == kTIFF_SpatialFrequencyResponse) ) {
						ok = FlipOECFSFRTable ( voidPtr, ps6Tag.dataLen, this->GetUns16 );
					}
					if ( ! ok ) this->DeleteTag ( ifd, ps6Tag.id );
					break;
				
				default:
					// ? XMP_Throw ( "Unexpected tag type", kXMPErr_InternalFailure );
					this->DeleteTag ( ifd, ps6Tag.id );
					break;
	
			}
		}
		
	}
	
}	// TIFF_FileWriter::ProcessPShop6IFD

// =================================================================================================
// TIFF_FileWriter::DetermineAppendInfo
// ====================================

#ifndef Trace_DetermineAppendInfo
	#define Trace_DetermineAppendInfo 0
#endif

XMP_Uns32 TIFF_FileWriter::DetermineAppendInfo ( XMP_Uns32 appendedOrigin,
												 bool      appendedIFDs[kTIFF_KnownIFDCount],
												 XMP_Uns32 newIFDOffsets[kTIFF_KnownIFDCount],
												 bool      appendAll /* = false */ )
{
	XMP_Uns32 appendedLength = 0;
	XMP_Assert ( (appendedOrigin & 1) == 0 );	// Make sure it is even.
	
	#if Trace_DetermineAppendInfo
	{
		printf ( "\nEntering TIFF_FileWriter::DetermineAppendInfo%s\n", (appendAll ? ", append all" : "") );
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
			InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
			printf ( "\n   IFD %d, origCount %d, map.size %d, origOffset %d (0x%X), origNextIFD %d (0x%X)",
					 ifd, thisIFD.origCount, thisIFD.tagMap.size(),
					 thisIFD.origOffset, thisIFD.origOffset, thisIFD.origNextIFD, thisIFD.origNextIFD );
			if ( thisIFD.changed ) printf ( ", changed" );
			if ( thisIFD.origCount < thisIFD.tagMap.size() ) printf ( ", should get appended" );
			printf ( "\n" );
			InternalTagMap::iterator tagPos;
			InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
			for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
				InternalTagInfo & thisTag = tagPos->second;
				printf ( "      Tag %d, dataOrOffset 0x%X, origLen %d, origOffset %d (0x%X)",
						 thisTag.id, thisTag.dataOrOffset, thisTag.origLen, thisTag.origOffset, thisTag.origOffset );
				if ( thisTag.changed ) printf ( ", changed" );
				if ( (thisTag.dataLen > thisTag.origLen) && (thisTag.dataLen > 4) ) printf ( ", should get appended" );
				printf ( "\n" );
			}
		}
		printf ( "\n" );
	}
	#endif
	
	// Determine which of the IFDs will be appended. If the Exif, GPS, or Interoperability IFDs are
	// appended, set dummy values for their offsets in the "owning" IFD. This must be done first
	// since this might cause the owning IFD to grow.
	
	if ( ! appendAll ) {
		for ( int i = 0; i < kTIFF_KnownIFDCount ;++i ) appendedIFDs[i] = false;
	} else {
		for ( int i = 0; i < kTIFF_KnownIFDCount ;++i ) appendedIFDs[i] = (this->containedIFDs[i].tagMap.size() > 0);
	}
	
	appendedIFDs[kTIFF_InteropIFD] |= (this->containedIFDs[kTIFF_InteropIFD].origCount <
									   this->containedIFDs[kTIFF_InteropIFD].tagMap.size());
	if ( appendedIFDs[kTIFF_InteropIFD] ) {
		this->SetTag_Long ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer, 0xABADABAD );
	}
	
	appendedIFDs[kTIFF_GPSInfoIFD] |= (this->containedIFDs[kTIFF_GPSInfoIFD].origCount <
									   this->containedIFDs[kTIFF_GPSInfoIFD].tagMap.size());
	if ( appendedIFDs[kTIFF_GPSInfoIFD] ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer, 0xABADABAD );
	}
	
	appendedIFDs[kTIFF_ExifIFD] |= (this->containedIFDs[kTIFF_ExifIFD].origCount <
								    this->containedIFDs[kTIFF_ExifIFD].tagMap.size());
	if ( appendedIFDs[kTIFF_ExifIFD] ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer, 0xABADABAD );
	}
	
	appendedIFDs[kTIFF_TNailIFD] |= (this->containedIFDs[kTIFF_TNailIFD].origCount <
									 this->containedIFDs[kTIFF_TNailIFD].tagMap.size());
	
	appendedIFDs[kTIFF_PrimaryIFD] |= (this->containedIFDs[kTIFF_PrimaryIFD].origCount <
									   this->containedIFDs[kTIFF_PrimaryIFD].tagMap.size());

	// The appended data (if any) will be a sequence of an IFD followed by its large values.
	// Determine the new offsets for the appended IFDs and tag values, and the total amount of
	// appended stuff. 
		
	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount ;++ifd ) {
	
		InternalIFDInfo& ifdInfo ( this->containedIFDs[ifd] );
		size_t tagCount = ifdInfo.tagMap.size();

		if ( ! (appendAll | ifdInfo.changed) ) continue;
		if ( tagCount == 0 ) continue;
		
		newIFDOffsets[ifd] = ifdInfo.origOffset;
		if ( appendedIFDs[ifd] ) {
			newIFDOffsets[ifd] = appendedOrigin + appendedLength;
			appendedLength += 6 + (12 * tagCount);
		}
		
		InternalTagMap::iterator tagPos = ifdInfo.tagMap.begin();
		InternalTagMap::iterator tagEnd = ifdInfo.tagMap.end();

		for ( ; tagPos != tagEnd; ++tagPos ) {

			InternalTagInfo & currTag ( tagPos->second );
			if ( (! (appendAll | currTag.changed)) || (currTag.dataLen <= 4) ) continue;

			if ( (currTag.dataLen <= currTag.origLen) && (! appendAll) ) {
				this->PutUns32 ( currTag.origOffset, &currTag.dataOrOffset );	// Reuse the old space.
			} else {
				this->PutUns32 ( (appendedOrigin + appendedLength), &currTag.dataOrOffset );	// Set the appended offset.
				appendedLength += ((currTag.dataLen + 1) & 0xFFFFFFFEUL);	// Round to an even size.
			}

		}
	
	}
	
	// If the Exif, GPS, or Interoperability IFDs get appended, update the tag values for their new offsets.
	
	if ( appendedIFDs[kTIFF_ExifIFD] ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer, newIFDOffsets[kTIFF_ExifIFD] );
	}
	if ( appendedIFDs[kTIFF_GPSInfoIFD] ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer, newIFDOffsets[kTIFF_GPSInfoIFD] );
	}
	if ( appendedIFDs[kTIFF_InteropIFD] ) {
		this->SetTag_Long ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer, newIFDOffsets[kTIFF_InteropIFD] );
	}
	
	#if Trace_DetermineAppendInfo
	{
		printf ( "Exiting TIFF_FileWriter::DetermineAppendInfo\n" );
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
			InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
			printf ( "\n   IFD %d, origCount %d, map.size %d, origOffset %d (0x%X), origNextIFD %d (0x%X)",
					 ifd, thisIFD.origCount, thisIFD.tagMap.size(),
					 thisIFD.origOffset, thisIFD.origOffset, thisIFD.origNextIFD, thisIFD.origNextIFD );
			if ( thisIFD.changed ) printf ( ", changed" );
			if ( appendedIFDs[ifd] ) printf ( ", will be appended at %d (0x%X)", newIFDOffsets[ifd], newIFDOffsets[ifd] );
			printf ( "\n" );
			InternalTagMap::iterator tagPos;
			InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
			for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
				InternalTagInfo & thisTag = tagPos->second;
				printf ( "      Tag %d, dataOrOffset 0x%X, origLen %d, origOffset %d (0x%X)",
						 thisTag.id, thisTag.dataOrOffset, thisTag.origLen, thisTag.origOffset, thisTag.origOffset );
				if ( thisTag.changed ) printf ( ", changed" );
				if ( (thisTag.dataLen > thisTag.origLen) && (thisTag.dataLen > 4) ) {
					XMP_Uns32 newOffset = this->GetUns32 ( &thisTag.dataOrOffset );
					printf ( ", will be appended at %d (0x%X)", newOffset, newOffset );
				}
				printf ( "\n" );
			}
		}
		printf ( "\n" );
	}
	#endif
	
	return appendedLength;
	
}	// TIFF_FileWriter::DetermineAppendInfo

// =================================================================================================
// TIFF_FileWriter::UpdateMemByAppend
// ==================================
//
// Normally we update TIFF in a conservative "by-append" manner. Changes are written in-place where
// they fit, anything requiring growth is appended to the end and the old space is abandoned. The
// end for memory-based TIFF is the end of the data block, the end for file-based TIFF is the end of
// the file. This update-by-append model has the advantage of not perturbing any hidden offsets, a
// common feature of proprietary MakerNotes.
//
// When doing the update-by-append we're only going to be modifying things that have changed. This
// means IFDs with changed, added, or deleted tags, and large values for changed or added tags. The
// IFDs and tag values are updated in-place if they fit, leaving holes in the stream if the new
// value is smaller than the old.

// ** Someday we might want to use the FreeOffsets and FreeByteCounts tags to track free space.
// ** Probably not a huge win in practice though, and the TIFF spec says they are not recommended
// ** for general interchange use.

void TIFF_FileWriter::UpdateMemByAppend ( XMP_Uns8** newStream_out, XMP_Uns32* newLength_out,
										  bool appendAll /* = false */, XMP_Uns32 extraSpace /* = 0 */ )
{
	bool appendedIFDs[kTIFF_KnownIFDCount];
	XMP_Uns32 newIFDOffsets[kTIFF_KnownIFDCount];
	XMP_Uns32 appendedOrigin = ((this->tiffLength + 1) & 0xFFFFFFFEUL);	// Start at an even offset.
	XMP_Uns32 appendedLength = DetermineAppendInfo ( appendedOrigin, appendedIFDs, newIFDOffsets, appendAll );

	// Allocate the new block of memory for the full stream. Copy the original stream. Write the
	// modified IFDs and values. Finally rebuild the internal IFD info and tag map.
	
	XMP_Uns32 newLength = appendedOrigin + appendedLength;
	XMP_Uns8* newStream = (XMP_Uns8*) malloc ( newLength + extraSpace );
	if ( newStream == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );

	memcpy ( newStream, this->memStream, this->tiffLength );	// AUDIT: Safe, malloc'ed newLength bytes above.
	if ( this->tiffLength < appendedOrigin ) {
		XMP_Assert ( appendedOrigin == (this->tiffLength + 1) );
		newStream[this->tiffLength] = 0;	// Clear the pad byte.
	}
	
	try {	// We might get exceptions from the next part and must delete newStream on the way out.
	
		// Write the modified IFDs and values. Rewrite the full IFD from scratch to make sure the
		// tags are now unique and sorted. Copy large changed values to their appropriate location.
		
		XMP_Uns32 appendedOffset = appendedOrigin;
		
		for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
		
			InternalIFDInfo& ifdInfo ( this->containedIFDs[ifd] );
			size_t tagCount = ifdInfo.tagMap.size();

			if ( ! (appendAll | ifdInfo.changed) ) continue;
			if ( tagCount == 0 ) continue;

			XMP_Uns8* ifdPtr = newStream + newIFDOffsets[ifd];
			
			if ( appendedIFDs[ifd] ) {
				XMP_Assert ( newIFDOffsets[ifd] == appendedOffset );
				appendedOffset += 6 + (12 * tagCount);
			}
			
			this->PutUns16 ( (XMP_Uns16)tagCount, ifdPtr );
			ifdPtr += 2;

			InternalTagMap::iterator tagPos = ifdInfo.tagMap.begin();
			InternalTagMap::iterator tagEnd = ifdInfo.tagMap.end();

			for ( ; tagPos != tagEnd; ++tagPos ) {

				InternalTagInfo & currTag ( tagPos->second );

				this->PutUns16 ( currTag.id, ifdPtr );
				ifdPtr += 2;
				this->PutUns16 ( currTag.type, ifdPtr );
				ifdPtr += 2;
				this->PutUns32 ( currTag.count, ifdPtr );
				ifdPtr += 4;

				*((XMP_Uns32*)ifdPtr) = currTag.dataOrOffset;

				if ( (appendAll | currTag.changed) && (currTag.dataLen > 4) ) {

					XMP_Uns32 valueOffset = this->GetUns32 ( &currTag.dataOrOffset );

					if ( (currTag.dataLen <= currTag.origLen) && (! appendAll) ) {
						XMP_Assert ( valueOffset == currTag.origOffset );
					} else {
						XMP_Assert ( valueOffset == appendedOffset );
						appendedOffset += ((currTag.dataLen + 1) & 0xFFFFFFFEUL);
					}

					if ( currTag.dataLen > (newLength - valueOffset) ) XMP_Throw ( "Buffer overrun", kXMPErr_InternalFailure );
					memcpy ( (newStream + valueOffset), currTag.dataPtr, currTag.dataLen );	// AUDIT: Protected by the above check.
					if ( (currTag.dataLen & 1) != 0 ) newStream[valueOffset+currTag.dataLen] = 0;

				}

				ifdPtr += 4;

			}
			
			this->PutUns32 ( ifdInfo.origNextIFD, ifdPtr );
			ifdPtr += 4;
		
		}
		
		XMP_Assert ( appendedOffset == newLength );
		
		// Back fill the offsets for the primary and thumnbail IFDs, if they are now appended.
		
		if ( appendedIFDs[kTIFF_PrimaryIFD] ) {
			this->PutUns32 ( newIFDOffsets[kTIFF_PrimaryIFD], (newStream + 4) );
		}
		
		if ( appendedIFDs[kTIFF_TNailIFD] ) {
			size_t primaryIFDCount = this->containedIFDs[kTIFF_PrimaryIFD].tagMap.size();
			XMP_Uns32 tnailRefOffset = newIFDOffsets[kTIFF_PrimaryIFD] + 2 + (12 * primaryIFDCount);
			this->PutUns32 ( newIFDOffsets[kTIFF_TNailIFD], (newStream + tnailRefOffset) );
		}
	
	} catch ( ... ) {
	
		free ( newStream );
		throw;
	
	}
	
	*newStream_out = newStream;
	*newLength_out = newLength;
	
}	// TIFF_FileWriter::UpdateMemByAppend

// =================================================================================================
// TIFF_FileWriter::DetermineVisibleLength
// =======================================

XMP_Uns32 TIFF_FileWriter::DetermineVisibleLength()
{
	XMP_Uns32 visibleLength = 8;	// Start with the TIFF header size.

	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
	
		InternalIFDInfo& ifdInfo ( this->containedIFDs[ifd] );
		size_t tagCount = ifdInfo.tagMap.size();
		if ( tagCount == 0 ) continue;

		visibleLength += 6 + (12 * tagCount);

		InternalTagMap::iterator tagPos = ifdInfo.tagMap.begin();
		InternalTagMap::iterator tagEnd = ifdInfo.tagMap.end();

		for ( ; tagPos != tagEnd; ++tagPos ) {
			InternalTagInfo & currTag ( tagPos->second );
			if ( currTag.dataLen > 4 ) visibleLength += ((currTag.dataLen + 1) & 0xFFFFFFFE);	// ! Round to even lengths.
		}
	
	}
	
	return visibleLength;
	
}	// TIFF_FileWriter::DetermineVisibleLength

// =================================================================================================
// TIFF_FileWriter::UpdateMemByRewrite
// ===================================
//
// Normally we update TIFF in a conservative "by-append" manner. Changes are written in-place where
// they fit, anything requiring growth is appended to the end and the old space is abandoned. The
// end for memory-based TIFF is the end of the data block, the end for file-based TIFF is the end of
// the file. This update-by-append model has the advantage of not perturbing any hidden offsets, a
// common feature of proprietary MakerNotes.
//
// The condenseStream parameter can be used to rewrite the full stream instead of appending. This
// will discard any MakerNote tag and risks breaking offsets that are hidden. This can be necessary
// though to try to make the TIFF fit in a JPEG file.
//
// We don't do most of the actual rewrite here. We set things up so that UpdateMemByAppend can be
// called to append onto a bare TIFF header. Additional hidden offsets are then handled here.
//
// These tags are recognized as being hidden offsets when composing a condensed stream:
//    273 - StripOffsets, lengths in tag 279
//    288 - FreeOffsets, lengths in tag 289
//    324 - TileOffsets, lengths in tag 325
//    330 - SubIFDs, lengths within the IFDs (Plus subIFD values and possible chaining!)
//    513 - JPEGInterchangeFormat, length in tag 514
//    519 - JPEGQTables, each table is 64 bytes
//    520 - JPEGDCTables, lengths ???
//    521 - JPEGACTables, lengths ???
// Some of these will handled and kept, some will be thrown out, some will cause the rewrite to fail.
//
// The hidden offsets for the Exif, GPS, and Interoperability IFDs (tags 34665, 34853, and 40965)
// are handled by the code in DetermineAppendInfo, which is called from UpdateMemByAppend, which is
// called from here.

// ! So far, a memory-based TIFF rewrite would only be done for the Exif portion of a JPEG file.
// ! In which case we're probably OK to handle JPEGInterchangeFormat (used for compressed thumbnails)
// ! and complain about any of the other hidden offset tags.

// tag	count	type

// 273		n	short or long
// 279		n	short or long
// 288		n	long
// 289		n	long
// 324		n	long
// 325		n	short or long

// 330		n	long

// 513		1	long
// 514		1	long

// 519		n	long
// 520		n	long
// 521		n	long

static XMP_Uns16 kNoGoTags[] =
	{
		kTIFF_StripOffsets,		// 273	*** Should be handled?
		kTIFF_StripByteCounts,	// 279	*** Should be handled?
		kTIFF_FreeOffsets,		// 288	*** Should be handled?
		kTIFF_FreeByteCounts,	// 289	*** Should be handled?
		kTIFF_TileOffsets,		// 324	*** Should be handled?
		kTIFF_TileByteCounts,	// 325	*** Should be handled?
		kTIFF_SubIFDs,			// 330	*** Should be handled?
		kTIFF_JPEGQTables,		// 519
		kTIFF_JPEGDCTables,		// 520
		kTIFF_JPEGACTables,		// 521
		0xFFFF	// Must be last as a sentinel.
	};

static XMP_Uns16 kBanishedTags[] =
	{
		kTIFF_MakerNote,	// *** Should someday support MakerNoteSafety.
		0xFFFF	// Must be last as a sentinel.
	};

struct SimpleHiddenContentInfo {
	XMP_Uns8  ifd;
	XMP_Uns16 offsetTag, lengthTag;
};

struct SimpleHiddenContentLocations {
	XMP_Uns32 length, oldOffset, newOffset;
	SimpleHiddenContentLocations() : length(0), oldOffset(0), newOffset(0) {};
};

enum { kSimpleHiddenContentCount = 1 };

static const SimpleHiddenContentInfo kSimpleHiddenContentInfo [kSimpleHiddenContentCount] =
	{
		{ kTIFF_TNailIFD, kTIFF_JPEGInterchangeFormat, kTIFF_JPEGInterchangeFormatLength }
	};

// -------------------------------------------------------------------------------------------------

void TIFF_FileWriter::UpdateMemByRewrite ( XMP_Uns8** newStream_out, XMP_Uns32* newLength_out ) 
{
	const InternalTagInfo* tagInfo;
	
	// Check for tags that we don't tolerate because they have data we can't (or refuse to) find.
	
	for ( XMP_Uns8 ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
		for ( int i = 0; kNoGoTags[i] != 0xFFFF; ++i ) {
			tagInfo = this->FindTagInIFD ( ifd, kNoGoTags[i] );
			if ( tagInfo != 0 ) XMP_Throw ( "Tag not tolerated for TIFF rewrite", kXMPErr_Unimplemented );
		}
	}
	
	// Delete unwanted tags. 
	
	for ( XMP_Uns8 ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {
		for ( int i = 0; kBanishedTags[i] != 0xFFFF; ++i ) {
			this->DeleteTag ( ifd, kBanishedTags[i] );
		}
	}
	
	// Make sure the "pointer" tags for the Exif, GPS, and Interop IFDs exist. The order is
	// important, adding the Interop pointer can cause the Exif IFD to exist.
	
	if ( ! this->containedIFDs[kTIFF_InteropIFD].tagMap.empty() ) {
		this->SetTag_Long ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer, 0xABADABAD );
	}
	
	if ( ! this->containedIFDs[kTIFF_GPSInfoIFD].tagMap.empty() ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer, 0xABADABAD );
	}
	
	if ( ! this->containedIFDs[kTIFF_ExifIFD].tagMap.empty() ) {
		this->SetTag_Long ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer, 0xABADABAD );
	}

	// Determine the offsets and additional size for the hidden offset content. Set the offset tags
	// to the new offset.
	
	XMP_Uns32 hiddenContentLength = 0;
	XMP_Uns32 hiddenContentOrigin = this->DetermineVisibleLength();
	
	SimpleHiddenContentLocations hiddenLocations [kSimpleHiddenContentCount];
	
	for ( int i = 0; i < kSimpleHiddenContentCount; ++i ) {

		const SimpleHiddenContentInfo & hiddenInfo ( kSimpleHiddenContentInfo[i] );
		
		bool haveLength = this->GetTag_Integer ( hiddenInfo.ifd, hiddenInfo.lengthTag, &hiddenLocations[i].length );
		bool haveOffset = this->GetTag_Integer ( hiddenInfo.ifd, hiddenInfo.offsetTag, &hiddenLocations[i].oldOffset );
		if ( haveLength != haveOffset ) XMP_Throw ( "Unpaired simple hidden content tag", kXMPErr_BadTIFF );
		if ( (! haveLength) || (hiddenLocations[i].length == 0) ) continue;

		hiddenLocations[i].newOffset = hiddenContentOrigin + hiddenContentLength;
		this->SetTag_Long ( hiddenInfo.ifd, hiddenInfo.offsetTag, hiddenLocations[i].newOffset );
		hiddenContentLength += ((hiddenLocations[i].length + 1) & 0xFFFFFFFE);	// ! Round up for even offsets.

	}
	
	// Save any old memory stream for the content behind hidden offsets. Setup a bare TIFF header.

	XMP_Uns8* oldStream = this->memStream;

	XMP_Uns8 bareTIFF [8];
	if ( this->bigEndian ) {
		bareTIFF[0] = 0x4D; bareTIFF[1] = 0x4D; bareTIFF[2] = 0x00; bareTIFF[3] = 0x2A;
	} else {
		bareTIFF[0] = 0x49; bareTIFF[1] = 0x49; bareTIFF[2] = 0x2A; bareTIFF[3] = 0x00;
	}
	*((XMP_Uns32*)&bareTIFF[4]) = 0;
	
	this->memStream = &bareTIFF[0];
	this->tiffLength = sizeof ( bareTIFF );
	this->ownedStream = false;

	// Call UpdateMemByAppend to write the new stream, telling it to append everything.
	
	this->UpdateMemByAppend ( newStream_out, newLength_out, true, hiddenContentLength );

	// Copy the hidden content and update the output stream length;

	XMP_Assert ( *newLength_out == hiddenContentOrigin );
	*newLength_out += hiddenContentLength;
	
	for ( int i = 0; i < kSimpleHiddenContentCount; ++i ) {

		if ( hiddenLocations[i].length == 0 ) continue;

		XMP_Uns8* srcPtr  = oldStream + hiddenLocations[i].oldOffset;
		XMP_Uns8* destPtr = *newStream_out + hiddenLocations[i].newOffset;
		memcpy ( destPtr, srcPtr, hiddenLocations[i].length );	// AUDIT: Safe copy, not user data, computed length.

	}
		
}	// TIFF_FileWriter::UpdateMemByRewrite

// =================================================================================================
// TIFF_FileWriter::UpdateMemoryStream
// ===================================
//
// Normally we update TIFF in a conservative "by-append" manner. Changes are written in-place where
// they fit, anything requiring growth is appended to the end and the old space is abandoned. The
// end for memory-based TIFF is the end of the data block, the end for file-based TIFF is the end of
// the file. This update-by-append model has the advantage of not perturbing any hidden offsets, a
// common feature of proprietary MakerNotes.
//
// The condenseStream parameter can be used to rewrite the full stream instead of appending. This
// will discard any MakerNote tags and risks breaking offsets that are hidden. This can be necessary
// though to try to make the TIFF fit in a JPEG file.

XMP_Uns32 TIFF_FileWriter::UpdateMemoryStream ( void** dataPtr, bool condenseStream /* = false */ ) 
{
	if ( this->fileParsed ) XMP_Throw ( "Not memory based", kXMPErr_EnforceFailure );
	
	if ( ! this->changed ) {
		if ( dataPtr != 0 ) *dataPtr = this->memStream;
		return this->tiffLength;
	}
	
	bool nowEmpty = true;
	for ( size_t i = 0; i < kTIFF_KnownIFDCount; ++i ) {
		if ( ! this->containedIFDs[i].tagMap.empty() ) {
			nowEmpty = false;
			break;
		}
	}
	
	XMP_Uns8* newStream = 0;
	XMP_Uns32 newLength = 0;
	
	if ( nowEmpty ) {
	
		this->DeleteExistingInfo();	// Prepare for an empty reparse.
	
	} else {

		if ( this->tiffLength == 0 ) {	// ! An empty parse does set this->memParsed.
			condenseStream = true;		// Makes "conjured" TIFF take the full rewrite path.
		}
	
		if ( condenseStream ) this->changed = true;	// A prior regular call would have cleared this->changed.
		
		if ( condenseStream ) {
			this->UpdateMemByRewrite ( &newStream, &newLength );
		} else {
			this->UpdateMemByAppend ( &newStream, &newLength );
		}
	
	}
	
	// Parse the revised stream. This is the cleanest way to rebuild the tag map.
	
	this->ParseMemoryStream ( newStream, newLength, kDoNotCopyData );
	this->ownedStream = true;	// ! We really do own the stream.
	
	if ( dataPtr != 0 ) *dataPtr = this->memStream;
	return this->tiffLength;
	
}	// TIFF_FileWriter::UpdateMemoryStream

// =================================================================================================
// TIFF_FileWriter::UpdateFileStream
// =================================
//
// Updating a file stream is done in the same general manner as updating a memory stream, the intro
// comments for UpdateMemoryStream largely apply. The file update happens in 3 phases:
//   1. Determine which IFDs will be appended, and the new offsets for the appended IFDs and tags.
//   2. Do the in-place update for the IFDs and tag values that fit.
//   3. Append the IFDs and tag values that grow.
//
// The file being updated must match the file that was previously parsed. Offsets and lengths saved
// when parsing are used to decide if something can be updated in-place or must be appended.

// *** The general linked structure of TIFF makes it very difficult to process the file in a single
// *** sequential pass. This implementation uses a simple seek/write model for the in-place updates.
// *** In the future we might want to consider creating a map of what gets updated, allowing use of
// *** a possibly more efficient buffered model.

// ** Someday we might want to use the FreeOffsets and FreeByteCounts tags to track free space.
// ** Probably not a huge win in practice though, and the TIFF spec says they are not recommended
// ** for general interchange use.

#ifndef Trace_UpdateFileStream
	#define Trace_UpdateFileStream 0
#endif

void TIFF_FileWriter::UpdateFileStream ( LFA_FileRef fileRef ) 
{
	if ( this->memParsed ) XMP_Throw ( "Not file based", kXMPErr_EnforceFailure );
	if ( ! this->changed ) return;
	
	XMP_Int64 origLength = LFA_Measure ( fileRef );
	if ( (origLength >> 32) != 0 ) XMP_Throw ( "TIFF files can't exceed 4GB", kXMPErr_BadTIFF );
	
	bool appendedIFDs[kTIFF_KnownIFDCount];
	XMP_Uns32 newIFDOffsets[kTIFF_KnownIFDCount];
	
	#if Trace_UpdateFileStream
		printf ( "\nStarting update of TIFF file stream\n" );
	#endif

	XMP_Uns32 appendedOrigin = (XMP_Uns32)origLength;
	if ( (appendedOrigin & 1) != 0 ) {
		++appendedOrigin;	// Start at an even offset.
		LFA_Seek ( fileRef, 0, SEEK_END );
		LFA_Write ( fileRef, "\0", 1 );
	}

	XMP_Uns32 appendedLength = DetermineAppendInfo ( appendedOrigin, appendedIFDs, newIFDOffsets );
	if ( appendedLength > (0xFFFFFFFFUL - appendedOrigin) ) XMP_Throw ( "TIFF files can't exceed 4GB", kXMPErr_BadTIFF );

	// Do the in-place update for the IFDs and tag values that fit. This part does separate seeks
	// and writes for the IFDs and values. Things to be updated can be anywhere in the file.
	
	// *** This might benefit from a map of the in-place updates. This would allow use of a possibly
	// *** more efficient sequential I/O model. Could even incorporate the safe update file copying.
	
	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {

		InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
		if ( ! thisIFD.changed ) continue;

		// In order to get a little bit of locality, write the IFD first then the changed tags that
		// have large values and fit in-place.
		
		if ( ! appendedIFDs[ifd] ) {
			#if Trace_UpdateFileStream
				printf ( "  Updating IFD %d in-place at offset %d (0x%X)\n", ifd, thisIFD.origOffset, thisIFD.origOffset );
			#endif
			LFA_Seek ( fileRef, thisIFD.origOffset, SEEK_SET );
			this->WriteFileIFD ( fileRef, thisIFD );
		}
			
		InternalTagMap::iterator tagPos;
		InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
		
		for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
			InternalTagInfo & thisTag = tagPos->second;
			if ( (! thisTag.changed) || (thisTag.dataLen <= 4) || (thisTag.dataLen > thisTag.origLen) ) continue;
			#if Trace_UpdateFileStream
				printf ( "    Updating tag %d in IFD %d in-place at offset %d (0x%X)\n", thisTag.id, ifd, thisTag.origOffset, thisTag.origOffset );
			#endif
			LFA_Seek ( fileRef, thisTag.origOffset, SEEK_SET );
			LFA_Write ( fileRef, thisTag.dataPtr, thisTag.dataLen );
		}

	}

	// Append the IFDs and tag values that grow.

	XMP_Int64 fileEnd = LFA_Seek ( fileRef, 0, SEEK_END );
	XMP_Assert ( fileEnd == appendedOrigin );
	
	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {

		InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
		if ( ! thisIFD.changed ) continue;
		
		if ( appendedIFDs[ifd] ) {
			#if Trace_UpdateFileStream
				printf ( "  Updating IFD %d by append at offset %d (0x%X)\n", ifd, newIFDOffsets[ifd], newIFDOffsets[ifd] );
			#endif
			XMP_Assert ( newIFDOffsets[ifd] == LFA_Measure(fileRef) );
			this->WriteFileIFD ( fileRef, thisIFD );
		}
			
		InternalTagMap::iterator tagPos;
		InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
		
		for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
			InternalTagInfo & thisTag = tagPos->second;
			if ( (! thisTag.changed) || (thisTag.dataLen <= 4) || (thisTag.dataLen <= thisTag.origLen) ) continue;
			#if Trace_UpdateFileStream
				XMP_Uns32 newOffset = this->GetUns32(&thisTag.dataOrOffset);
				printf ( "    Updating tag %d in IFD %d by append at offset %d (0x%X)\n", thisTag.id, ifd, newOffset, newOffset );
			#endif
			XMP_Assert ( this->GetUns32(&thisTag.dataOrOffset) == LFA_Measure(fileRef) );
			LFA_Write ( fileRef, thisTag.dataPtr, thisTag.dataLen );
			if ( (thisTag.dataLen & 1) != 0 ) LFA_Write ( fileRef, "\0", 1 );
		}

	}

	// Back-fill the offsets for the primary and thumnbail IFDs, if they are now appended.
	
	XMP_Uns32 newOffset;
	
	if ( appendedIFDs[kTIFF_PrimaryIFD] ) {
		this->PutUns32 ( newIFDOffsets[kTIFF_PrimaryIFD], &newOffset );
		#if TraceUpdateFileStream
			printf ( "  Back-filling offset of primary IFD, pointing to %d (0x%X)\n", newOffset, newOffset );
		#endif
		LFA_Seek ( fileRef, 4, SEEK_SET );
		LFA_Write ( fileRef, &newOffset, 4 );
	}
	
	InternalIFDInfo & primaryIFD = this->containedIFDs[kTIFF_PrimaryIFD];
	InternalIFDInfo & tnailIFD   = this->containedIFDs[kTIFF_TNailIFD];
	
	if ( appendedIFDs[kTIFF_TNailIFD] && (primaryIFD.origNextIFD == tnailIFD.origOffset) ) {

		size_t primaryIFDCount = primaryIFD.tagMap.size();
		XMP_Uns32 tnailRefOffset = newIFDOffsets[kTIFF_PrimaryIFD] + 2 + (12 * primaryIFDCount);

		this->PutUns32 ( newIFDOffsets[kTIFF_TNailIFD], &newOffset );
		#if TraceUpdateFileStream
			printf ( "  Back-filling offset of thumbnail IFD, offset at %d (0x%X), pointing to %d (0x%X)\n",
					 tnailRefOffset, tnailRefOffset, newOffset, newOffset );
		#endif
		LFA_Seek ( fileRef, tnailRefOffset, SEEK_SET );
		LFA_Write ( fileRef, &newOffset, 4 );
		
		primaryIFD.origNextIFD = newIFDOffsets[kTIFF_TNailIFD];	// ! Ought to be below, easier here.

	}
	
	// Reset the changed flags and original length/offset values. This simulates a reparse of the
	// updated file.
	
	for ( int ifd = 0; ifd < kTIFF_KnownIFDCount; ++ifd ) {

		InternalIFDInfo & thisIFD = this->containedIFDs[ifd];
		if ( ! thisIFD.changed ) continue;

		thisIFD.changed = false;
		thisIFD.origCount  = thisIFD.tagMap.size();
		thisIFD.origOffset = newIFDOffsets[ifd];
			
		InternalTagMap::iterator tagPos;
		InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();
		
		for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {
			InternalTagInfo & thisTag = tagPos->second;
			if ( ! thisTag.changed ) continue;
			thisTag.changed = false;
			thisTag.origLen = thisTag.dataLen;
			if ( thisTag.origLen > 4 ) thisTag.origOffset = this->GetUns32 ( &thisTag.dataOrOffset );
		}

	}

	this->tiffLength = (XMP_Uns32) LFA_Measure ( fileRef );
	LFA_Seek ( fileRef, 0, SEEK_END );	// Can't hurt.
	
	#if Trace_UpdateFileStream
		printf ( "\nFinished update of TIFF file stream\n" );
	#endif

}	// TIFF_FileWriter::UpdateFileStream

// =================================================================================================
// TIFF_FileWriter::WriteFileIFD
// =============================

void TIFF_FileWriter::WriteFileIFD ( LFA_FileRef fileRef, InternalIFDInfo & thisIFD )
{
	XMP_Uns16 tagCount;
	this->PutUns16 ( thisIFD.tagMap.size(), &tagCount );
	LFA_Write ( fileRef, &tagCount, 2 );
	
	InternalTagMap::iterator tagPos;
	InternalTagMap::iterator tagEnd = thisIFD.tagMap.end();

	for ( tagPos = thisIFD.tagMap.begin(); tagPos != tagEnd; ++tagPos ) {

		InternalTagInfo & thisTag = tagPos->second;
		RawIFDEntry ifdEntry;

		this->PutUns16 ( thisTag.id, &ifdEntry.id );
		this->PutUns16 ( thisTag.type, &ifdEntry.type );
		this->PutUns32 ( thisTag.count, &ifdEntry.count );
		ifdEntry.dataOrOffset = thisTag.dataOrOffset;	// ! Already in stream endianness.

		LFA_Write ( fileRef, &ifdEntry, sizeof(ifdEntry) );
		XMP_Assert ( sizeof(ifdEntry) == 12 );

	}
	
	XMP_Uns32 nextIFD;
	this->PutUns32 ( thisIFD.origNextIFD, &nextIFD );
	LFA_Write ( fileRef, &nextIFD, 4 );

}	// TIFF_FileWriter::WriteFileIFD
