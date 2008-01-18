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
/// \file TIFF_MemoryReader.cpp
/// \brief Implementation of the memory-based read-only TIFF_Manager.
///
/// The read-only forms of TIFF_Manager are derived from TIFF_Reader. The GetTag methods are common
/// implementations in TIFF_Reader. The parsing code is different in the TIFF_MemoryReader and 
/// TIFF_FileReader constructors. There are also separate destructors to release captured info.
///
/// The read-only implementations use runtime data that is simple tweaks on the stored form. The
/// memory-based reader has one block of data for the whole TIFF stream. The file-based reader has
/// one for each IFD, plus one for the collected non-local data for each IFD. Otherwise the logic
/// is the same in both cases.
///
/// The count for each IFD is extracted and a pointer set to the first entry in each IFD (serving as
/// a normal C array pointer). The IFD entries are tweaked as follows:
///
/// \li The id and type fields are converted to native values.
/// \li The count field is converted to a native byte count.
/// \li If the data is not inline the offset is converted to a pointer.
///
/// The tag values, whether inline or not, are not converted to native values. The values returned
/// from the GetTag methods are converted on the fly. The id, type, and count fields are easier to
/// convert because their types are fixed. They are used more, and more valuable to convert.
// =================================================================================================

// =================================================================================================
// TIFF_MemoryReader::SortIFD
// ==========================
//
// Does a fairly simple minded insertion-like sort. This sort is not going to be optimal for edge
// cases like and IFD with lots of duplicates.

// *** Might be better done using read and write pointers and two loops. The first loop moves out
// *** of order tags by a modified bubble sort, shifting the middle down one at a time in the loop.
// *** The first loop stops when a duplicate is hit. The second loop continues by moving the tail
// *** entries up to the appropriate slot.

void TIFF_MemoryReader::SortIFD ( TweakedIFDInfo* thisIFD )
{

	XMP_Uns16 tagCount = thisIFD->count;
	TweakedIFDEntry* ifdEntries = thisIFD->entries;
	XMP_Uns16 prevTag = ifdEntries[0].id;
	
	for ( size_t i = 1; i < tagCount; ++i ) {
		
		XMP_Uns16 thisTag = ifdEntries[i].id;

		if ( thisTag > prevTag ) {

			// In proper order.
			prevTag = thisTag;

		} else if ( thisTag == prevTag ) {

			// Duplicate tag, keep the 2nd copy, move the tail of the array up, prevTag is unchanged.
			memcpy ( &ifdEntries[i-1], &ifdEntries[i], 12*(tagCount-i) );	// AUDIT: Safe, moving tail forward, i >= 1.
			--tagCount;
			--i; // ! Don't move forward in the array, we've moved the unseen part up.

		} else if ( thisTag < prevTag ) {

			// Out of order, move this tag up, prevTag is unchanged. Might still be a duplicate!
			XMP_Int32 j;	// ! Need a signed value.
			for ( j = i-1; j >= 0; --j ) {
				if ( ifdEntries[j].id <= thisTag ) break;
			}
			
			if ( (j >= 0) && (ifdEntries[j].id == thisTag) ) {

				// Out of order duplicate, move it to position j, move the tail of the array up.
				ifdEntries[j] = ifdEntries[i];
				memcpy ( &ifdEntries[i], &ifdEntries[i+1], 12*(tagCount-(i+1)) );	// AUDIT: Safe, moving tail forward, i >= 1.
				--tagCount;
				--i; // ! Don't move forward in the array, we've moved the unseen part up.

			} else {

				// Move the out of order entry to position j+1, move the middle of the array down.
				TweakedIFDEntry temp = ifdEntries[i];
				++j;	// ! So the insertion index becomes j.
				memcpy ( &ifdEntries[j+1], &ifdEntries[j], 12*(i-j) );	// AUDIT: Safe, moving less than i entries to a location before i.
				ifdEntries[j] = temp;

			}

		}
		
	}
	
	thisIFD->count = tagCount;	// Save the final count.
	
}	// TIFF_MemoryReader::SortIFD

// =================================================================================================
// TIFF_MemoryReader::GetIFD
// =========================

bool TIFF_MemoryReader::GetIFD ( XMP_Uns8 ifd, TagInfoMap* ifdMap ) const
{
	if ( ifd > kTIFF_LastRealIFD ) XMP_Throw ( "Invalid IFD requested", kXMPErr_InternalFailure );
	const TweakedIFDInfo* thisIFD = &containedIFDs[ifd];
	
	if ( ifdMap != 0 ) ifdMap->clear();
	if ( thisIFD->count == 0 ) return false;
	
	if ( ifdMap != 0 ) {
		
		for ( size_t i = 0; i < thisIFD->count; ++i ) {

			TweakedIFDEntry* thisTag = &(thisIFD->entries[i]);

			TagInfo info ( thisTag->id, thisTag->type, 0, 0, thisTag->bytes );
			info.count = info.dataLen / kTIFF_TypeSizes[info.type];
			if ( info.dataLen <= 4 ) {
				info.dataPtr = &(thisTag->dataOrPtr);
			} else {
				info.dataPtr = (const void*)(thisTag->dataOrPtr);
			}

			(*ifdMap)[info.id] = info;

		}
	
	}
	
	return true;

}	// TIFF_MemoryReader::GetIFD

// =================================================================================================
// TIFF_MemoryReader::FindTagInIFD
// ===============================

const TIFF_MemoryReader::TweakedIFDEntry* TIFF_MemoryReader::FindTagInIFD ( XMP_Uns8 ifd, XMP_Uns16 id ) const
{
	if ( ifd == kTIFF_KnownIFD ) {
		// ... lookup the tag in the known tag map
	}
	
	if ( ifd > kTIFF_LastRealIFD ) XMP_Throw ( "Invalid IFD requested", kXMPErr_InternalFailure );
	const TweakedIFDInfo* thisIFD = &containedIFDs[ifd];
	
	if ( thisIFD->count == 0 ) return 0;
	
	XMP_Uns32 spanLength = thisIFD->count;
	const TweakedIFDEntry* spanBegin = &(thisIFD->entries[0]);
	
	while ( spanLength > 1 ) {

		XMP_Uns32 halfLength = spanLength >> 1;	// Since spanLength > 1, halfLength > 0.
		const TweakedIFDEntry* spanMiddle = spanBegin + halfLength;
		
		// There are halfLength entries below spanMiddle, then the spanMiddle entry, then
		// spanLength-halfLength-1 entries above spanMiddle (which can be none).
		
		if ( spanMiddle->id == id ) {
			spanBegin = spanMiddle;
			break;
		} else if ( spanMiddle->id > id ) {
			spanLength = halfLength;	// Discard the middle.
		} else {
			spanBegin = spanMiddle;	// Keep a valid spanBegin for the return check, don't use spanMiddle+1.
			spanLength -= halfLength;
		}
		
	}
	
	if ( spanBegin->id != id ) spanBegin = 0;
	return spanBegin;

}	// TIFF_MemoryReader::FindTagInIFD

// =================================================================================================
// TIFF_MemoryReader::GetValueOffset
// =================================

XMP_Uns32 TIFF_MemoryReader::GetValueOffset ( XMP_Uns8 ifd, XMP_Uns16 id ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return 0;
	
	XMP_Uns8 * valuePtr = (XMP_Uns8*) thisTag->dataOrPtr;
	if ( thisTag->bytes <= 4 ) valuePtr = (XMP_Uns8*) &(thisTag->dataOrPtr);
	
	return (valuePtr - this->tiffStream);
	
}	// TIFF_MemoryReader::GetValueOffset

// =================================================================================================
// TIFF_MemoryReader::GetTag
// =========================

bool TIFF_MemoryReader::GetTag ( XMP_Uns8 ifd, XMP_Uns16 id, TagInfo* info ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	
	if ( info != 0 ) {

		info->id = thisTag->id;
		info->type = thisTag->type;
		info->count = thisTag->bytes / kTIFF_TypeSizes[thisTag->type];
		info->dataLen = thisTag->bytes;
		
		if ( thisTag->bytes <= 4 ) {
			info->dataPtr = &(thisTag->dataOrPtr);
		} else {
			info->dataPtr = (const void*)(thisTag->dataOrPtr);
		}

	}
	
	return true;
	
}	// TIFF_MemoryReader::GetTag

// =================================================================================================
// TIFF_MemoryReader::GetTag_Integer
// =================================

bool TIFF_MemoryReader::GetTag_Integer ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns32* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	
	if ( data != 0 ) {
		if ( thisTag->type == kTIFF_ShortType ) {
			if ( thisTag->bytes != 2 ) return false;	// Wrong count.
			*data = this->GetUns16 ( &(thisTag->dataOrPtr) );
		} else if ( thisTag->type == kTIFF_LongType ) {
			if ( thisTag->bytes != 4 ) return false;	// Wrong count.
			*data = this->GetUns32 ( &(thisTag->dataOrPtr) );
		} else {
			return false;
		}
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Integer

// =================================================================================================
// TIFF_MemoryReader::GetTag_Byte
// ==============================

bool TIFF_MemoryReader::GetTag_Byte ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns8* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_ByteType) || (thisTag->bytes != 1) ) return false;
	
	if ( data != 0 ) {
		*data = * ( (XMP_Uns8*) (&(thisTag->dataOrPtr)) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Byte

// =================================================================================================
// TIFF_MemoryReader::GetTag_SByte
// ===============================

bool TIFF_MemoryReader::GetTag_SByte ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int8* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SByteType) || (thisTag->bytes != 1) ) return false;
	
	if ( data != 0 ) {
		*data = * ( (XMP_Int8*) (&(thisTag->dataOrPtr)) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_SByte

// =================================================================================================
// TIFF_MemoryReader::GetTag_Short
// ===============================

bool TIFF_MemoryReader::GetTag_Short ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns16* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_ShortType) || (thisTag->bytes != 2) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetUns16 ( &(thisTag->dataOrPtr) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Short

// =================================================================================================
// TIFF_MemoryReader::GetTag_SShort
// ================================

bool TIFF_MemoryReader::GetTag_SShort ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int16* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SShortType) || (thisTag->bytes != 2) ) return false;
	
	if ( data != 0 ) {
		*data = (XMP_Int16) this->GetUns16 ( &(thisTag->dataOrPtr) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_SShort

// =================================================================================================
// TIFF_MemoryReader::GetTag_Long
// ==============================

bool TIFF_MemoryReader::GetTag_Long ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Uns32* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_LongType) || (thisTag->bytes != 4) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetUns32 ( &(thisTag->dataOrPtr) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Long

// =================================================================================================
// TIFF_MemoryReader::GetTag_SLong
// ===============================

bool TIFF_MemoryReader::GetTag_SLong ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_Int32* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SLongType) || (thisTag->bytes != 4) ) return false;
	
	if ( data != 0 ) {
		*data = (XMP_Int32) this->GetUns32 ( &(thisTag->dataOrPtr) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_SLong

// =================================================================================================
// TIFF_MemoryReader::GetTag_Rational
// ==================================

bool TIFF_MemoryReader::GetTag_Rational ( XMP_Uns8 ifd, XMP_Uns16 id, Rational* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_RationalType) || (thisTag->bytes != 8) ) return false;
	
	if ( data != 0 ) {
		XMP_Uns32* dataPtr = (XMP_Uns32*)thisTag->dataOrPtr;
		data->num = this->GetUns32 ( dataPtr );
		data->denom = this->GetUns32 ( dataPtr+1 );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Rational

// =================================================================================================
// TIFF_MemoryReader::GetTag_SRational
// ===================================

bool TIFF_MemoryReader::GetTag_SRational ( XMP_Uns8 ifd, XMP_Uns16 id, SRational* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_SRationalType) || (thisTag->bytes != 8) ) return false;
	
	if ( data != 0 ) {
		XMP_Uns32* dataPtr = (XMP_Uns32*)thisTag->dataOrPtr;
		data->num = (XMP_Int32) this->GetUns32 ( dataPtr );
		data->denom = (XMP_Int32) this->GetUns32 ( dataPtr+1 );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_SRational

// =================================================================================================
// TIFF_MemoryReader::GetTag_Float
// ===============================

bool TIFF_MemoryReader::GetTag_Float ( XMP_Uns8 ifd, XMP_Uns16 id, float* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_FloatType) || (thisTag->bytes != 4) ) return false;
	
	if ( data != 0 ) {
		*data = this->GetFloat ( &(thisTag->dataOrPtr) );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Float

// =================================================================================================
// TIFF_MemoryReader::GetTag_Double
// ================================

bool TIFF_MemoryReader::GetTag_Double ( XMP_Uns8 ifd, XMP_Uns16 id, double* data ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( (thisTag->type != kTIFF_DoubleType) || (thisTag->bytes != 8) ) return false;
	
	if ( data != 0 ) {
		double* dataPtr = (double*)thisTag->dataOrPtr;
		*data = this->GetDouble ( dataPtr );
	}
	
	return true;

}	// TIFF_MemoryReader::GetTag_Double

// =================================================================================================
// TIFF_MemoryReader::GetTag_ASCII
// ===============================

bool TIFF_MemoryReader::GetTag_ASCII ( XMP_Uns8 ifd, XMP_Uns16 id, XMP_StringPtr* dataPtr, XMP_StringLen* dataLen ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( thisTag->type != kTIFF_ASCIIType ) return false;
	
	if ( dataPtr != 0 ) {
		*dataPtr = (XMP_StringPtr)thisTag->dataOrPtr;
		if ( thisTag->bytes <= 4 ) *dataPtr = (XMP_StringPtr)(&thisTag->dataOrPtr);
	}
	
	if ( dataLen != 0 ) *dataLen = thisTag->bytes;
	
	return true;

}	// TIFF_MemoryReader::GetTag_ASCII

// =================================================================================================
// TIFF_MemoryReader::GetTag_EncodedString
// =======================================

bool TIFF_MemoryReader::GetTag_EncodedString ( XMP_Uns8 ifd, XMP_Uns16 id, std::string* utf8Str ) const
{
	const TweakedIFDEntry* thisTag = this->FindTagInIFD ( ifd, id );
	if ( thisTag == 0 ) return false;
	if ( thisTag->type != kTIFF_UndefinedType ) return false;
	
	if ( utf8Str == 0 ) return true;	// Return true if the converted string is not wanted.
	
	bool ok = this->DecodeString ( (void*)thisTag->dataOrPtr, thisTag->bytes, utf8Str );
	return ok;

}	// TIFF_MemoryReader::GetTag_EncodedString

// =================================================================================================
// TIFF_MemoryReader::ParseMemoryStream
// ====================================

// *** Need to tell TIFF/Exif from TIFF/EP, DNG files are the latter.

void TIFF_MemoryReader::ParseMemoryStream ( const void* data, XMP_Uns32 length, bool copyData /* = true */ )
{
	// Get rid of any current TIFF.
	
	if ( this->ownedStream ) free ( this->tiffStream );
	this->ownedStream = false;
	this->tiffStream  = 0;
	this->tiffLength  = 0;
	
	for ( size_t i = 0; i < kTIFF_KnownIFDCount; ++i ) {
		this->containedIFDs[i].count = 0;
		this->containedIFDs[i].entries = 0;
	}
	
	if ( length == 0 ) return;

	// Allocate space for the full in-memory stream and copy it.
	
	if ( ! copyData ) {
		XMP_Assert ( ! this->ownedStream );
		this->tiffStream = (XMP_Uns8*) data;
	} else {
		if ( length > 100*1024*1024 ) XMP_Throw ( "Outrageous length for memory-based TIFF", kXMPErr_BadTIFF );
		this->tiffStream = (XMP_Uns8*) malloc(length);
		if ( this->tiffStream == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( this->tiffStream, data, length );	// AUDIT: Safe, malloc'ed length bytes above.
		this->ownedStream = true;
	}

	this->tiffLength = length;
	
	// Find and process the primary, Exif, GPS, and Interoperability IFDs.
	
	XMP_Uns32 primaryIFDOffset = this->CheckTIFFHeader ( this->tiffStream, length );
	XMP_Uns32 tnailIFDOffset   = 0;
	
	if ( primaryIFDOffset != 0 ) tnailIFDOffset = this->ProcessOneIFD ( primaryIFDOffset, kTIFF_PrimaryIFD );

	const TweakedIFDEntry* exifIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_ExifIFDPointer );
	if ( (exifIFDTag != 0) && (exifIFDTag->type == kTIFF_LongType) && (exifIFDTag->bytes == 4) ) {
		XMP_Uns32 exifOffset = this->GetUns32 ( &exifIFDTag->dataOrPtr );
		(void) this->ProcessOneIFD ( exifOffset, kTIFF_ExifIFD );
	}

	const TweakedIFDEntry* gpsIFDTag = this->FindTagInIFD ( kTIFF_PrimaryIFD, kTIFF_GPSInfoIFDPointer );
	if ( (gpsIFDTag != 0) && (gpsIFDTag->type == kTIFF_LongType) && (gpsIFDTag->bytes == 4) ) {
		XMP_Uns32 gpsOffset = this->GetUns32 ( &gpsIFDTag->dataOrPtr );
		(void) this->ProcessOneIFD ( gpsOffset, kTIFF_GPSInfoIFD );
	}

	const TweakedIFDEntry* interopIFDTag = this->FindTagInIFD ( kTIFF_ExifIFD, kTIFF_InteroperabilityIFDPointer );
	if ( (interopIFDTag != 0) && (interopIFDTag->type == kTIFF_LongType) && (interopIFDTag->bytes == 4) ) {
		XMP_Uns32 interopOffset = this->GetUns32 ( &interopIFDTag->dataOrPtr );
		(void) this->ProcessOneIFD ( interopOffset, kTIFF_InteropIFD );
	}
	
	// Process the thumbnail IFD. We only do this for Exif-compliant TIFF streams. Extract the
	// JPEG thumbnail image pointer (tag 513) for later use by GetTNailInfo.

	if ( (tnailIFDOffset != 0) && (this->containedIFDs[kTIFF_ExifIFD].count > 0) ) {
		(void) this->ProcessOneIFD ( tnailIFDOffset, kTIFF_TNailIFD );
		const TweakedIFDEntry* jpegInfo = FindTagInIFD ( kTIFF_TNailIFD, kTIFF_JPEGInterchangeFormat );
		if ( jpegInfo != 0 ) {
			XMP_Uns32 tnailImageOffset = this->GetUns32 ( &jpegInfo->dataOrPtr );
			this->jpegTNailPtr = (XMP_Uns8*)this->tiffStream + tnailImageOffset;
		}
	}

}	// TIFF_MemoryReader::ParseMemoryStream

// =================================================================================================
// TIFF_MemoryReader::ProcessOneIFD
// ================================

XMP_Uns32 TIFF_MemoryReader::ProcessOneIFD ( XMP_Uns32 ifdOffset, XMP_Uns8 ifd )
{
	TweakedIFDInfo& ifdInfo = this->containedIFDs[ifd];
	
	if ( (ifdOffset < 8) || (ifdOffset > (this->tiffLength - kEmptyIFDLength)) ) {
		XMP_Throw ( "Bad IFD offset", kXMPErr_BadTIFF );
	}
		
	XMP_Uns8* ifdPtr = this->tiffStream + ifdOffset;
	XMP_Uns16 ifdCount = this->GetUns16 ( ifdPtr );
	TweakedIFDEntry* ifdEntries = (TweakedIFDEntry*)(ifdPtr+2);

	if ( ifdCount >= 0x8000 ) XMP_Throw ( "Outrageous IFD count", kXMPErr_BadTIFF );
	if ( (ifdOffset + 2 + ifdCount*12 + 4) > this->tiffLength ) XMP_Throw ( "Out of bounds IFD", kXMPErr_BadTIFF );

	ifdInfo.count = ifdCount;
	ifdInfo.entries = ifdEntries;
	
	XMP_Int32 prevTag = -1;	// ! The GPS IFD has a tag 0, so we need a signed initial value.
	bool needsSorting = false;
	for ( size_t i = 0; i < ifdCount; ++i ) {
		
		TweakedIFDEntry* thisEntry = &ifdEntries[i];	// Tweak the IFD entry to be more useful.

		if ( ! this->nativeEndian ) {
			Flip2 ( &thisEntry->id );
			Flip2 ( &thisEntry->type );
			Flip4 ( &thisEntry->bytes );
		}
		
		if ( thisEntry->id <= prevTag ) needsSorting = true;
		prevTag = thisEntry->id;

		if ( (thisEntry->type < kTIFF_ByteType) || (thisEntry->type > kTIFF_LastType) ) continue;	// Bad type, skip this tag.

		thisEntry->bytes *= kTIFF_TypeSizes[thisEntry->type];
		if ( thisEntry->bytes > this->tiffLength ) XMP_Throw ( "Bad TIFF data size", kXMPErr_BadTIFF );
		if ( thisEntry->bytes > 4 ) {
			if ( ! this->nativeEndian ) Flip4 ( &thisEntry->dataOrPtr );
			thisEntry->dataOrPtr += (XMP_Uns32)this->tiffStream;
		}

	}
	
	ifdPtr += (2 + ifdCount*12);
	XMP_Uns32 nextIFDOffset = this->GetUns32 ( ifdPtr );
	
	if ( needsSorting ) SortIFD ( &ifdInfo );	// ! Don't perturb the ifdCount used to find the next IFD offset.
	
	return nextIFDOffset;

}	// TIFF_MemoryReader::ProcessOneIFD

// =================================================================================================
