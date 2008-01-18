// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2002-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "JPEG_Handler.hpp"

#include "TIFF_Support.hpp"
#include "PSIR_Support.hpp"
#include "IPTC_Support.hpp"
#include "ReconcileLegacy.hpp"

#include "MD5.h"

using namespace std;

// =================================================================================================
/// \file JPEG_Handler.cpp
/// \brief File format handler for JPEG.
///
/// This handler ...
///
// =================================================================================================

static const char * kExifSignatureString = "Exif\0\x00";
static const char * kExifSignatureAltStr = "Exif\0\xFF";
static const size_t kExifSignatureLength = 6;
static const size_t kExifMaxDataLength   = 0xFFFF - 2 - kExifSignatureLength;

static const char * kPSIRSignatureString = "Photoshop 3.0\0";
static const size_t kPSIRSignatureLength = 14;
static const size_t kPSIRMaxDataLength   = 0xFFFF - 2 - kPSIRSignatureLength;

static const char * kMainXMPSignatureString = "http://ns.adobe.com/xap/1.0/\0";
static const size_t kMainXMPSignatureLength = 29;

static const char * kExtXMPSignatureString = "http://ns.adobe.com/xmp/extension/\0";
static const size_t kExtXMPSignatureLength = 35;
static const size_t kExtXMPPrefixLength    = kExtXMPSignatureLength + 32 + 4 + 4;

typedef std::map < XMP_Uns32 /* offset */, std::string /* portion */ > ExtXMPPortions;

struct ExtXMPContent {
	XMP_Uns32 length;
	ExtXMPPortions portions;
	ExtXMPContent() : length(0) {};
	ExtXMPContent ( XMP_Uns32 _length ) : length(_length) {};
};

typedef std::map < JPEG_MetaHandler::GUID_32 /* guid */, ExtXMPContent /* content */ > ExtendedXMPInfo;

#ifndef Trace_UnlimitedJPEG
	#define Trace_UnlimitedJPEG 0
#endif

// =================================================================================================
// JPEG_MetaHandlerCTor
// ====================

XMPFileHandler * JPEG_MetaHandlerCTor ( XMPFiles * parent )
{
	return new JPEG_MetaHandler ( parent );

}	// JPEG_MetaHandlerCTor

// =================================================================================================
// JPEG_CheckFormat
// ================

// For JPEG we just check for the initial SOI standalone marker followed by any of the other markers
// that might, well, follow it. A more aggressive check might be to read 4KB then check for legit
// marker segments within that portion. Probably won't buy much, and thrashes the dCache more. We
// tolerate only a small amount of 0xFF padding between the SOI and following marker. This formally
// violates the rules of JPEG, but in practice there won't be any padding anyway.
//
// ! The CheckXyzFormat routines don't track the filePos, that is left to ScanXyzFile.

bool JPEG_CheckFormat ( XMP_FileFormat format,
	                    XMP_StringPtr  filePath,
                        LFA_FileRef    fileRef,
                        XMPFiles *     parent )
{
	IgnoreParam(format); IgnoreParam(filePath); IgnoreParam(parent);
	XMP_Assert ( format == kXMP_JPEGFile );

	IOBuffer ioBuf;
	
	LFA_Seek ( fileRef, 0, SEEK_SET );
	if ( ! CheckFileSpace ( fileRef, &ioBuf, 4 ) ) return false;	// We need at least 4, the buffer is filled anyway.
	
	// First look for the SOI standalone marker. Then skip all 0xFF bytes, padding plus the high
	// order byte of the next marker. Finally see if the next marker is legit.
	
	if ( ! CheckBytes ( ioBuf.ptr, "\xFF\xD8", 2 ) ) return false;
	ioBuf.ptr += 2;	// Move past the SOI.
	while ( (ioBuf.ptr < ioBuf.limit) && (*ioBuf.ptr == 0xFF) ) ++ioBuf.ptr;
	if ( ioBuf.ptr == ioBuf.limit ) return false;
	
	XMP_Uns8 id = *ioBuf.ptr;
	if ( id >= 0xDD ) return true;	// The most probable cases.
	if ( (id < 0xC0) || ((id & 0xF8) == 0xD0) || (id == 0xD8) || (id == 0xDA) || (id == 0xDC) ) return false;
	return true;
	
}	// JPEG_CheckFormat

// =================================================================================================
// JPEG_MetaHandler::JPEG_MetaHandler
// ==================================

JPEG_MetaHandler::JPEG_MetaHandler ( XMPFiles * _parent )
	: exifMgr(0), psirMgr(0), iptcMgr(0), skipReconcile(false)
{
	this->parent = _parent;
	this->handlerFlags = kJPEG_HandlerFlags;
	this->stdCharForm  = kXMP_Char8Bit;

}	// JPEG_MetaHandler::JPEG_MetaHandler

// =================================================================================================
// JPEG_MetaHandler::~JPEG_MetaHandler
// ===================================

JPEG_MetaHandler::~JPEG_MetaHandler()
{

	if ( exifMgr != 0 ) delete ( exifMgr );
	if ( psirMgr != 0 ) delete ( psirMgr );
	if ( iptcMgr != 0 ) delete ( iptcMgr );
	
}	// JPEG_MetaHandler::~JPEG_MetaHandler

// =================================================================================================
// TableOrDataMarker
// =================
//
// Returns true if the marker is for a table or data marker segment:
//   FFC4 - DHT
//   FFCC - DAC
//   FFDB - DQT
//   FFDC - DNL
//   FFDD - DRI
//   FFDE - DHP
//   FFDF - EXP
//   FFEn - APPn
//   FFFE - COM

static inline bool TableOrDataMarker ( XMP_Uns16 marker )
{

	if ( (marker & 0xFFF0) == 0xFFE0 ) return true;	// APPn is probably the most common case.
	
	if ( marker < 0xFFC4 ) return false;
	if ( marker == 0xFFC4 ) return true;

	if ( marker < 0xFFCC ) return false;
	if ( marker == 0xFFCC ) return true;

	if ( marker < 0xFFDB ) return false;
	if ( marker <= 0xFFDF ) return true;

	if ( marker < 0xFFFE ) return false;
	if ( marker == 0xFFFE ) return true;
	
	return false;

}	// TableOrDataMarker

// =================================================================================================
// JPEG_MetaHandler::CacheFileData
// ===============================
//
// Look for the Exif metadata, Photoshop image resources, and XMP in a JPEG (JFIF) file. The native
// thumbnail is inside the Exif. The general layout of a JPEG file is:
//    SOI marker, 2 bytes, 0xFFD8
//    Marker segments for tables and metadata
//    SOFn marker segment
//    Image data
//    EOI marker, 2 bytes, 0xFFD9
//
// Each marker segment begins with a 2 byte big endian marker and a 2 byte big endian length. The
// length includes the 2 bytes of the length field but not the marker. The high order byte of a 
// marker is 0xFF, the low order byte tells what kind of marker. A marker can be preceeded by any
// number of 0xFF fill bytes, however there are no alignment constraints.
//
// There are virtually no constraints on the order of the marker segments before the SOFn. A reader
// must be prepared to handle any order.
//
// The Exif metadata is in an APP1 marker segment with a 6 byte signature string of "Exif\0\0" at
// the start of the data. The rest of the data is a TIFF stream.
//
// The Photoshop image resources are in an APP13 marker segment with a 14 byte signature string of
// "Photoshop 3.0\0". The rest of the data is a sequence of image resources.
//
// The main XMP is in an APP1 marker segment with a 29 byte signature string of
// "http://ns.adobe.com/xap/1.0/\0". The rest of the data is the serialized XMP packet. This is the
// only XMP if everything fits within the 64KB limit for marker segment data. If not, there will be
// a series of XMP extension segments.
//
// Each XMP extension segment is an APP1 marker segment whose data contains:
//    - A 35 byte signature string of "http://ns.adobe.com/xmp/extension/\0".
//    - A 128 bit GUID stored as 32 ASCII hex digits, capital A-F, no nul termination.
//    - A 32 bit unsigned integer length for the full extended XMP serialization.
//    - A 32 bit unsigned integer offset for this portion of the extended XMP serialization.
//    - A portion of the extended XMP serialization, up to about 65400 bytes (at most 65458).
//
// A reader must be prepared to encounter the extended XMP portions out of order. Also to encounter
// defective files that have differing extended XMP according to the GUID. The main XMP contains the
// GUID for the associated extended XMP.

// *** This implementation simply returns when invalid JPEG is encountered. Should we throw instead?

void JPEG_MetaHandler::CacheFileData()
{
	LFA_FileRef      fileRef    = this->parent->fileRef;
	XMP_PacketInfo & packetInfo = this->packetInfo;

	size_t	  segLen;
	bool      ok;
	IOBuffer ioBuf;
	
	XMP_AbortProc abortProc  = this->parent->abortProc;
	void *        abortArg   = this->parent->abortArg;
	const bool    checkAbort = (abortProc != 0);
	
	ExtendedXMPInfo extXMP;
	
	XMP_Assert ( (! this->containsXMP) && (! this->containsTNail) );
	// Set containsXMP to true here only if the standard XMP packet is found.
	
	XMP_Assert ( kPSIRSignatureLength == (strlen(kPSIRSignatureString) + 1) );
	XMP_Assert ( kMainXMPSignatureLength == (strlen(kMainXMPSignatureString) + 1) );
	XMP_Assert ( kExtXMPSignatureLength == (strlen(kExtXMPSignatureString) + 1) );
	
	// -------------------------------------------------------------------------------------------
	// Look for any of the Exif, PSIR, main XMP, or extended XMP marker segments. Quit when we hit
	// an SOFn, EOI, or invalid/unexpected marker.

	LFA_Seek ( fileRef, 2, SEEK_SET );	// Skip the SOI. The JPEG header has already been verified.
	ioBuf.filePos = 2;
	RefillBuffer ( fileRef, &ioBuf );
	
	while ( true ) {

		if ( checkAbort && abortProc(abortArg) ) {
			XMP_Throw ( "JPEG_MetaHandler::CacheFileData - User abort", kXMPErr_UserAbort );
		}
	
		if ( ! CheckFileSpace ( fileRef, &ioBuf, 2 ) ) return;

		if ( *ioBuf.ptr != 0xFF ) return;	// All valid markers have a high byte of 0xFF.
		while ( *ioBuf.ptr == 0xFF ) {	// Skip padding 0xFF bytes and the marker's high byte.
			++ioBuf.ptr;
			if ( ! CheckFileSpace ( fileRef, &ioBuf, 1 ) ) return;
		}
		
		XMP_Uns16 marker = 0xFF00 + *ioBuf.ptr;
		
		if ( marker == 0xFFED ) {
		
			// This is an APP13 marker, is it the Photoshop image resources?
			
			++ioBuf.ptr;	// Move ioBuf.ptr to the marker segment length field.
			if ( ! CheckFileSpace ( fileRef, &ioBuf, 2 ) ) return;
			
			segLen = GetUns16BE ( ioBuf.ptr );
			if ( segLen < 2 ) return;	// Invalid JPEG.

			ioBuf.ptr += 2;	// Move ioBuf.ptr to the marker segment content.
			segLen -= 2;	// Adjust segLen to count just the content portion.
			
			ok = CheckFileSpace ( fileRef, &ioBuf, kPSIRSignatureLength );
			if ( ok && (segLen >= kPSIRSignatureLength) &&
				 CheckBytes ( ioBuf.ptr, kPSIRSignatureString, kPSIRSignatureLength ) ) {
			
				// This is the Photoshop image resources, cache the contents.

				ioBuf.ptr += kPSIRSignatureLength;	// Move ioBuf.ptr to the image resources.
				segLen -= kPSIRSignatureLength;	// Adjust segLen to count just the image resources.
				ok = CheckFileSpace ( fileRef, &ioBuf, segLen );	// Buffer the full content portion.
				if ( ! ok ) return;	// Must be a truncated file.

				this->psirContents.assign ( (XMP_StringPtr)ioBuf.ptr, segLen );
				ioBuf.ptr += segLen;
			
			} else {
			
				// This is the not Photoshop image resources, skip the marker segment's content.

				if ( segLen <= size_t(ioBuf.limit - ioBuf.ptr) ) {
					ioBuf.ptr += segLen;	// The next marker is in this buffer.
				} else {
					// The next marker is beyond this buffer, RefillBuffer assumes we're doing sequential reads.
					size_t skipCount = segLen - (ioBuf.limit - ioBuf.ptr);	// The amount to move beyond this buffer.
					ioBuf.filePos = LFA_Seek ( fileRef, skipCount, SEEK_CUR );
					ioBuf.ptr = ioBuf.limit;		// No data left in the buffer.
				}

			}
			
			continue;	// Move on to the next marker.
			
		} else if ( marker == 0xFFE1 ) {
		
			// This is an APP1 marker, is it the Exif, main XMP, or extended XMP?
			// ! Check in that order, which happens to be increasing signature string length.
			
			++ioBuf.ptr;	// Move ioBuf.ptr to the marker segment length field.
			if ( ! CheckFileSpace ( fileRef, &ioBuf, 2 ) ) return;
			
			segLen = GetUns16BE ( ioBuf.ptr );
			if ( segLen < 2 ) return;	// Invalid JPEG.

			ioBuf.ptr += 2;	// Move ioBuf.ptr to the marker segment content.
			segLen -= 2;	// Adjust segLen to count just the content portion.
			
			// Check for the Exif APP1 marker segment.
			
			ok = CheckFileSpace ( fileRef, &ioBuf, kExifSignatureLength );
			if ( ok && (segLen >= kExifSignatureLength) &&
				 (CheckBytes ( ioBuf.ptr, kExifSignatureString, kExifSignatureLength ) ||
				  CheckBytes ( ioBuf.ptr, kExifSignatureAltStr, kExifSignatureLength )) ) {
			
				// This is the Exif metadata, cache the contents.

				ioBuf.ptr += kExifSignatureLength;	// Move ioBuf.ptr to the TIFF stream.
				segLen -= kExifSignatureLength;	// Adjust segLen to count just the TIFF stream.
				ok = CheckFileSpace ( fileRef, &ioBuf, segLen );	// Buffer the full content portion.
				if ( ! ok ) return;	// Must be a truncated file.

				this->exifContents.assign ( (XMP_StringPtr)ioBuf.ptr, segLen );
				ioBuf.ptr += segLen;
				
				continue;	// Move on to the next marker.
				
			}
			
			// Check for the main XMP APP1 marker segment.
			
			ok = CheckFileSpace ( fileRef, &ioBuf, kMainXMPSignatureLength );
			if ( ok && (segLen >= kMainXMPSignatureLength) &&
				 CheckBytes ( ioBuf.ptr, kMainXMPSignatureString, kMainXMPSignatureLength ) ) {
			
				// This is the main XMP, cache the contents.

				ioBuf.ptr += kMainXMPSignatureLength;	// Move ioBuf.ptr to the XMP Packet.
				segLen -= kMainXMPSignatureLength;	// Adjust segLen to count just the XMP Packet.
				ok = CheckFileSpace ( fileRef, &ioBuf, segLen );	// Buffer the full content portion.
				if ( ! ok ) return;	// Must be a truncated file.
				
				this->packetInfo.offset = ioBuf.filePos + (ioBuf.ptr - &ioBuf.data[0]);
				this->packetInfo.length = segLen;
				this->packetInfo.padSize   = 0;				// Assume for now, set these properly in ProcessXMP.
				this->packetInfo.charForm  = kXMP_CharUnknown;
				this->packetInfo.writeable = true;

				this->xmpPacket.assign ( (XMP_StringPtr)ioBuf.ptr, segLen );
				ioBuf.ptr += segLen;	// ! Set this->packetInfo.offset first!
				
				this->containsXMP = true;	// Found the standard XMP packet.
				continue;	// Move on to the next marker.
				
			}
			
			// Check for an extension XMP APP1 marker segment.
			
			ok = CheckFileSpace ( fileRef, &ioBuf, kExtXMPPrefixLength );	// ! The signature, GUID, length, and offset.
			if ( ok && (segLen >= kExtXMPPrefixLength) &&
				 CheckBytes ( ioBuf.ptr, kExtXMPSignatureString, kExtXMPSignatureLength ) ) {
			
				// This is a portion of the extended XMP, cache the contents. This is complicated by
				// the need to tolerate files where the extension portions are not in order. The
				// local ExtendedXMPInfo map uses the GUID as the key and maps that to a struct that
				// has the full length and a map of the known portions. This known portion map uses
				// the offset of the portion as the key and maps that to a string. Only fully seen
				// extended XMP streams are kept, the right one gets picked in ProcessXMP.
				
				segLen -= kExtXMPPrefixLength;	// Adjust segLen to count just the XMP stream portion.

				ioBuf.ptr += kExtXMPSignatureLength;	// Move ioBuf.ptr to the GUID.
				GUID_32 guid;
				XMP_Assert ( sizeof(guid.data) == 32 );
				memcpy ( &guid.data[0], ioBuf.ptr, sizeof(guid.data) );	// AUDIT: Use of sizeof(guid.data) is safe.
				
				ioBuf.ptr += 32;	// Move ioBuf.ptr to the length and offset.
				XMP_Uns32 fullLen = GetUns32BE ( ioBuf.ptr );
				XMP_Uns32 offset  = GetUns32BE ( ioBuf.ptr+4 );
				
				ioBuf.ptr += 8;	// Move ioBuf.ptr to the XMP stream portion.
				
				#if Trace_UnlimitedJPEG
					printf ( "New extended XMP portion: fullLen %d, offset %d, GUID %.32s\n", fullLen, offset, guid.data );
				#endif
				
				// Find the ExtXMPContent for this GUID, and the string for this portion's offset.
				
				ExtendedXMPInfo::iterator guidPos = extXMP.find ( guid );
				if ( guidPos == extXMP.end() ) {
					ExtXMPContent newExtContent ( fullLen );
					guidPos = extXMP.insert ( extXMP.begin(), ExtendedXMPInfo::value_type ( guid, newExtContent ) );
				}
				
				ExtXMPPortions::iterator offsetPos;
				ExtXMPContent & extContent = guidPos->second;

				if ( extContent.portions.empty() ) {
					// When new create a full size offset 0 string, to which all in-order portions will get appended.
					offsetPos = extContent.portions.insert ( extContent.portions.begin(),
															 ExtXMPPortions::value_type ( 0, std::string() ) );
					offsetPos->second.reserve ( extContent.length );
				}

				// Try to append this portion to a logically contiguous preceeding one.

				if ( offset == 0 ) {
					offsetPos = extContent.portions.begin();
					XMP_Assert ( (offsetPos->first == 0) && (offsetPos->second.size() == 0) );
				} else {
					offsetPos = extContent.portions.lower_bound ( offset );
					--offsetPos;	// Back up to the portion whose offset is less than the new offset.
					if ( (offsetPos->first + offsetPos->second.size()) != offset ) {
						// Can't append, create a new portion.
						offsetPos = extContent.portions.insert ( extContent.portions.begin(),
																 ExtXMPPortions::value_type ( offset, std::string() ) );
					}
				}
				
				// Cache this portion of the extended XMP.
				
				std::string & extPortion = offsetPos->second;
				ok = CheckFileSpace ( fileRef, &ioBuf, segLen );	// Buffer the full content portion.
				if ( ! ok ) return;	// Must be a truncated file.
				extPortion.append ( (XMP_StringPtr)ioBuf.ptr, segLen );
				ioBuf.ptr += segLen;
				
				continue;	// Move on to the next marker.
				
			}
			
			// If we get here this is some other uninteresting APP1 marker segment, skip it.
			
			if ( segLen <= size_t(ioBuf.limit - ioBuf.ptr) ) {
				ioBuf.ptr += segLen;	// The next marker is in this buffer.
			} else {
				// The next marker is beyond this buffer, RefillBuffer assumes we're doing sequential reads.
				size_t skipCount = segLen - (ioBuf.limit - ioBuf.ptr);	// The amount to move beyond this buffer.
				ioBuf.filePos = LFA_Seek ( fileRef, skipCount, SEEK_CUR );
				ioBuf.ptr = ioBuf.limit;		// No data left in the buffer.
			}
		
		} else if ( TableOrDataMarker ( marker ) ) {
		
			// This is a non-terminating but uninteresting marker segment. Skip it.
			
			++ioBuf.ptr;	// Move ioBuf.ptr to the marker segment length field.
			if ( ! CheckFileSpace ( fileRef, &ioBuf, 2 ) ) return;
			
			segLen = GetUns16BE ( ioBuf.ptr );	// Remember that the length includes itself.
			if ( segLen < 2 ) return;		// Invalid JPEG.

			if ( segLen <= size_t(ioBuf.limit - ioBuf.ptr) ) {
				ioBuf.ptr += segLen;	// The next marker is in this buffer.
			} else {
				// The next marker is beyond this buffer, RefillBuffer assumes we're doing sequential reads.
				size_t skipCount = segLen - (ioBuf.limit - ioBuf.ptr);	// The amount to move beyond this buffer.
				ioBuf.filePos = LFA_Seek ( fileRef, skipCount, SEEK_CUR );
				ioBuf.ptr = ioBuf.limit;		// No data left in the buffer.
			}
			
			continue;	// Move on to the next marker.

		} else {

			break;	// This is a terminating marker of some sort.
		
		}
	
	}

	if ( ! extXMP.empty() ) {
	
		// We have extended XMP. Find out which ones are complete, collapse them into a single
		// string, and save them for ProcessXMP.
		
		ExtendedXMPInfo::iterator guidPos = extXMP.begin();
		ExtendedXMPInfo::iterator guidEnd = extXMP.end();
		
		for ( ; guidPos != guidEnd; ++guidPos ) {
		
			ExtXMPContent & thisContent = guidPos->second;
			ExtXMPPortions::iterator partZero = thisContent.portions.begin();
			ExtXMPPortions::iterator partEnd  = thisContent.portions.end();
			ExtXMPPortions::iterator partPos  = partZero;
		
			#if Trace_UnlimitedJPEG
				printf ( "Extended XMP portions for GUID %.32s, full length %d\n",
					     guidPos->first.data, guidPos->second.length );
				printf ( "  Offset %d, length %d, next offset %d\n",
						 partZero->first, partZero->second.size(), (partZero->first + partZero->second.size()) );
			#endif
			
			for ( ++partPos; partPos != partEnd; ++partPos ) {
				#if Trace_UnlimitedJPEG
					printf ( "  Offset %d, length %d, next offset %d\n",
							 partPos->first, partPos->second.size(), (partPos->first + partPos->second.size()) );
				#endif
				if ( partPos->first != partZero->second.size() ) break;	// Quit if not contiguous.
				partZero->second.append ( partPos->second );
			}
			
			if ( (partPos == partEnd) && (partZero->first == 0) && (partZero->second.size() == thisContent.length) ) {
				// This is a complete extended XMP stream.
				this->extendedXMP.insert ( ExtendedXMPMap::value_type ( guidPos->first, partZero->second ) );
				#if Trace_UnlimitedJPEG
					printf ( "Full extended XMP for GUID %.32s, full length %d\n",
							 guidPos->first.data, partZero->second.size() );
				#endif
			}
					
		}
	
	}
	
}	// JPEG_MetaHandler::CacheFileData

// =================================================================================================
// JPEG_MetaHandler::ProcessTNail
// ==============================

void JPEG_MetaHandler::ProcessTNail()
{

	XMP_Assert ( ! this->processedTNail );
	this->processedTNail = true;	// Make sure we only come through here once.
	this->containsTNail = false;	// Set it to true after all of the info is gathered.
	
	if ( this->exifMgr == 0 ) {	// Thumbnails only need the Exif, not the PSIR or IPTC.
		bool readOnly = ((this->parent->openFlags & kXMPFiles_OpenForUpdate) == 0);
		if ( readOnly ) {	// *** Could reduce heap usage by not copying in TIFF_MemoryReader.
			this->exifMgr = new TIFF_MemoryReader();
		} else {
			this->exifMgr = new TIFF_FileWriter();
		}
		this->exifMgr->ParseMemoryStream ( this->exifContents.c_str(), this->exifContents.size() );
	}

	this->containsTNail = this->exifMgr->GetTNailInfo ( &this->tnailInfo );
	if ( this->containsTNail ) this->tnailInfo.fileFormat = this->parent->format;

}	// JPEG_MetaHandler::ProcessTNail

// =================================================================================================
// JPEG_MetaHandler::ProcessXMP
// ============================
//
// Process the raw XMP and legacy metadata that was previously cached.

void JPEG_MetaHandler::ProcessXMP()
{
	
	XMP_Assert ( ! this->processedXMP );
	this->processedXMP = true;	// Make sure we only come through here once.
	
	// Create the PSIR and IPTC handlers, even if there is no legacy. They might be needed for updates.
	
	XMP_Assert ( (this->psirMgr == 0) && (this->iptcMgr == 0) );	// ProcessTNail might create the exifMgr.

	bool readOnly = ((this->parent->openFlags & kXMPFiles_OpenForUpdate) == 0);
	
	if ( readOnly ) {
		if ( this->exifMgr == 0 ) this->exifMgr = new TIFF_MemoryReader();
		this->psirMgr = new PSIR_MemoryReader();
		this->iptcMgr = new IPTC_Reader();	// ! Parse it later.
	} else {
		if ( this->exifMgr == 0 ) this->exifMgr = new TIFF_FileWriter();
		this->psirMgr = new PSIR_FileWriter();
		this->iptcMgr = new IPTC_Writer();	// ! Parse it later.
	}

	// Set up everything for the legacy import, but don't do it yet. This lets us do a forced legacy
	// import if the XMP packet gets parsing errors.

	bool found;
	bool haveExif = (! this->exifContents.empty());
	bool haveIPTC = false;
	
	RecJTP_LegacyPriority lastLegacy = kLegacyJTP_None;

	TIFF_Manager & exif = *this->exifMgr;	// Give the compiler help in recognizing non-aliases.
	PSIR_Manager & psir = *this->psirMgr;
	IPTC_Manager & iptc = *this->iptcMgr;

	if ( haveExif ) {
		exif.ParseMemoryStream ( this->exifContents.c_str(), this->exifContents.size() );
	}
	
	if ( ! this->psirContents.empty() ) {
		psir.ParseMemoryResources ( this->psirContents.c_str(), this->psirContents.size() );
	}
	
	// Determine the last-legacy priority and do the reconciliation. For JPEG files, the relevant
	// legacy priorities (ignoring Mac pnot and ANPA resources) are:
	//	kLegacyJTP_PSIR_OldCaption - highest
	//	kLegacyJTP_PSIR_IPTC
	//	kLegacyJTP_JPEG_TIFF_Tags
	//	kLegacyJTP_None - lowest

	found = psir.GetImgRsrc ( kPSIR_OldCaption, 0 );
	if ( ! found ) found = psir.GetImgRsrc ( kPSIR_OldCaptionPStr, 0 );
	if ( found ) {
		haveIPTC = true;
		lastLegacy = kLegacyJTP_PSIR_OldCaption;
	}

	PSIR_Manager::ImgRsrcInfo iptcInfo;
	found = psir.GetImgRsrc ( kPSIR_IPTC, &iptcInfo );
	if ( found ) {
		haveIPTC = true;
		iptc.ParseMemoryDataSets ( iptcInfo.dataPtr, iptcInfo.dataLen );
		if ( lastLegacy < kLegacyJTP_PSIR_IPTC ) lastLegacy = kLegacyJTP_PSIR_IPTC;
	}
	
	if ( lastLegacy < kLegacyJTP_JPEG_TIFF_Tags ) {
		found = exif.GetTag ( kTIFF_PrimaryIFD, kTIFF_ImageDescription, 0 );
		if ( ! found ) found = exif.GetTag ( kTIFF_PrimaryIFD, kTIFF_Artist, 0 );
		if ( ! found ) found = exif.GetTag ( kTIFF_PrimaryIFD, kTIFF_Copyright, 0 );
		if ( found ) lastLegacy = kLegacyJTP_JPEG_TIFF_Tags;
	}
	
	XMP_OptionBits options = 0;
	if ( this->containsXMP ) options |= k2XMP_FileHadXMP;
	if ( haveExif ) options |= k2XMP_FileHadExif;
	if ( haveIPTC ) options |= k2XMP_FileHadIPTC;
	
	// Process the main XMP packet. If it fails to parse, do a forced legacy import but still throw
	// an exception. This tells the caller that an error happened, but gives them recovered legacy
	// should they want to proceed with that.

	if ( ! this->xmpPacket.empty() ) {
		XMP_Assert ( this->containsXMP );
		// Common code takes care of packetInfo.charForm, .padSize, and .writeable.
		XMP_StringPtr packetStr = this->xmpPacket.c_str();
		XMP_StringLen packetLen = this->xmpPacket.size();
		try {
			this->xmpObj.ParseFromBuffer ( packetStr, packetLen );
		} catch ( ... ) {
			XMP_ClearOption ( options, k2XMP_FileHadXMP );
			ImportJTPtoXMP ( kXMP_JPEGFile, lastLegacy, &exif, psir, &iptc, &this->xmpObj, options );
			throw;	// ! Rethrow the exception, don't absorb it.
		}
	}

	// Process the extended XMP if it has a matching GUID.

	if ( ! this->extendedXMP.empty() ) {
	
		bool found;
		GUID_32 g32;
		std::string extGUID, extPacket;
		ExtendedXMPMap::iterator guidPos = this->extendedXMP.end();

		found = this->xmpObj.GetProperty ( kXMP_NS_XMP_Note, "HasExtendedXMP", &extGUID, 0 );
		if ( found && (extGUID.size() == sizeof(g32.data)) ) {
			XMP_Assert ( sizeof(g32.data) == 32 );
			memcpy ( g32.data, extGUID.c_str(), sizeof(g32.data) );	// AUDIT: Use of sizeof(g32.data) is safe.
			guidPos = this->extendedXMP.find ( g32 );
			this->xmpObj.DeleteProperty ( kXMP_NS_XMP_Note, "HasExtendedXMP" );	// ! Must only be in the file.
			#if Trace_UnlimitedJPEG
				printf ( "%s extended XMP for GUID %s\n",
					     ((guidPos != this->extendedXMP.end()) ? "Found" : "Missing"), extGUID.c_str() );
			#endif
		}
		
		if ( guidPos != this->extendedXMP.end() ) {
			try {
				XMP_StringPtr extStr = guidPos->second.c_str();
				XMP_StringLen extLen = guidPos->second.size();
				SXMPMeta extXMP ( extStr, extLen );
				SXMPUtils::MergeFromJPEG ( &this->xmpObj, extXMP );
			} catch ( ... ) {
				// Ignore failures, let the rest of the XMP and legacy be kept.
			}
		}

	}
	
	// Process the legacy metadata.

	ImportJTPtoXMP ( kXMP_JPEGFile, lastLegacy, &exif, psir, &iptc, &this->xmpObj, options );
	if ( haveExif | haveIPTC ) this->containsXMP = true;	// Assume we had something for the XMP.
	
}	// JPEG_MetaHandler::ProcessXMP

// =================================================================================================
// JPEG_MetaHandler::UpdateFile
// ============================

void JPEG_MetaHandler::UpdateFile ( bool doSafeUpdate )
{
	XMP_Assert ( ! doSafeUpdate );	// This should only be called for "unsafe" updates.
	
	// Decide whether to do an in-place update. This can only happen if all of the following are true:
	//	- There is a standard packet in the file.
	//	- There is no extended XMP in the file.
	//	- The are no changes to the legacy Exif or PSIR portions. (The IPTC is in the PSIR.)
	//	- The new XMP can fit in the old space, without extensions.

	ExportXMPtoJTP ( kXMP_JPEGFile, &this->xmpObj, this->exifMgr, this->psirMgr, this->iptcMgr );
	
	XMP_Int64 oldPacketOffset = this->packetInfo.offset;
	XMP_Int32 oldPacketLength = this->packetInfo.length;
	
	if ( oldPacketOffset == kXMPFiles_UnknownOffset ) oldPacketOffset = 0;	// ! Simplify checks.
	if ( oldPacketLength == kXMPFiles_UnknownLength ) oldPacketLength = 0;
	
	bool doInPlace = (oldPacketOffset != 0) && (oldPacketLength != 0);	// ! Has old packet and new packet fits.
	
	if ( doInPlace && (! this->extendedXMP.empty()) ) doInPlace = false;
	
	if ( doInPlace && (this->exifMgr != 0) && (this->exifMgr->IsLegacyChanged()) ) doInPlace = false;
	if ( doInPlace && (this->psirMgr != 0) && (this->psirMgr->IsLegacyChanged()) ) doInPlace = false;

	if ( doInPlace ) {

		#if GatherPerformanceData
			sAPIPerf->back().extraInfo += ", JPEG in-place update";
		#endif
	
		LFA_FileRef liveFile = this->parent->fileRef;
		std::string & newPacket = this->xmpPacket;
	
		XMP_Assert ( newPacket.size() == (size_t)oldPacketLength );	// ! Done by common PutXMP logic.
		
		LFA_Seek ( liveFile, oldPacketOffset, SEEK_SET );
		LFA_Write ( liveFile, newPacket.c_str(), newPacket.size() );
	
	} else {

		#if GatherPerformanceData
			sAPIPerf->back().extraInfo += ", JPEG copy update";
		#endif
	
		std::string origPath = this->parent->filePath;
		LFA_FileRef origRef  = this->parent->fileRef;
		
		std::string updatePath;
		LFA_FileRef updateRef = 0;
		
		CreateTempFile ( origPath, &updatePath, kCopyMacRsrc );
		updateRef = LFA_Open ( updatePath.c_str(), 'w' );
	
		this->parent->filePath = updatePath;
		this->parent->fileRef  = updateRef;
		
		try {
			XMP_Assert ( ! this->skipReconcile );
			this->skipReconcile = true;
			this->WriteFile ( origRef, origPath );
			this->skipReconcile = false;
		} catch ( ... ) {
			this->skipReconcile = false;
			LFA_Close ( updateRef );
			this->parent->filePath = origPath;
			this->parent->fileRef  = origRef;
			throw;
		}
	
		LFA_Close ( origRef );
		LFA_Delete ( origPath.c_str() );
	
		LFA_Close ( updateRef );
		LFA_Rename ( updatePath.c_str(), origPath.c_str() );
		this->parent->filePath = origPath;
		this->parent->fileRef = 0;

	}
	
	this->needsUpdate = false;

}	// JPEG_MetaHandler::UpdateFile

// =================================================================================================
// JPEG_MetaHandler::WriteFile
// ===========================
//
// The metadata parts of a JPEG file are APP1 marker segments for Exif and XMP, and an APP13 marker
// segment for Photoshop image resources which contain the IPTC. Corresponding marker segments in
// the source file are ignored, other parts of the source file are copied. Any initial APP0 marker
// segments are copied first. Then the new Exif, XMP, and PSIR marker segments are written. Then the
// rest of the file is copied, skipping the old Exif, XMP, and PSIR. The checking for old metadata
// stops at the first SOFn marker.

// *** What about Mac resources?

void JPEG_MetaHandler::WriteFile ( LFA_FileRef sourceRef, const std::string & sourcePath )
{
	LFA_FileRef destRef = this->parent->fileRef;
	
	XMP_AbortProc abortProc  = this->parent->abortProc;
	void *        abortArg   = this->parent->abortArg;
	const bool    checkAbort = (abortProc != 0);

	XMP_Uns16 marker;
	size_t	  segLen;	// ! Must be a size to hold at least 64k+2.
	IOBuffer  ioBuf;
	XMP_Uns32 first4;
	
	XMP_Assert ( kIOBufferSize >= (2 + 64*1024) );	// Enough for a marker plus maximum contents.

	if ( LFA_Measure ( sourceRef ) == 0 ) return;	// Tolerate empty files.
	LFA_Seek ( sourceRef, 0, SEEK_SET );
	LFA_Truncate (destRef, 0 );

	if ( ! skipReconcile ) {
		ExportXMPtoJTP ( kXMP_JPEGFile, &this->xmpObj, this->exifMgr, this->psirMgr, this->iptcMgr );
	}
	
	RefillBuffer ( sourceRef, &ioBuf );
	if ( ! CheckFileSpace ( sourceRef, &ioBuf, 4 ) ) {
		XMP_Throw ( "JPEG must have at least SOI and EOI markers", kXMPErr_BadJPEG );
	}
	
	marker = GetUns16BE ( ioBuf.ptr );
	if ( marker != 0xFFD8 ) XMP_Throw ( "Missing SOI marker", kXMPErr_BadJPEG );
	LFA_Write ( destRef, ioBuf.ptr, 2 );
	ioBuf.ptr += 2;
	
	// Copy the leading APP0 marker segments.
	
	while ( true ) {

		if ( checkAbort && abortProc(abortArg) ) {
			XMP_Throw ( "JPEG_MetaHandler::WriteFile - User abort", kXMPErr_UserAbort );
		}
	
		if ( ! CheckFileSpace ( sourceRef, &ioBuf, 2 ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );
		marker = GetUns16BE ( ioBuf.ptr );
		if ( marker == 0xFFFF ) {
			LFA_Write ( destRef, ioBuf.ptr, 1 );	// Copy the 0xFF pad byte.
			++ioBuf.ptr;
			continue;
		}

		if ( marker != 0xFFE0 ) break;
	
		if ( ! CheckFileSpace ( sourceRef, &ioBuf, 4 ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );
		segLen = GetUns16BE ( ioBuf.ptr+2 );
		segLen += 2;	// ! Don't do above in case machine does 16 bit "+".
	
		if ( ! CheckFileSpace ( sourceRef, &ioBuf, segLen ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );
		LFA_Write ( destRef, ioBuf.ptr, segLen );
		ioBuf.ptr += segLen;
		
	}

	// Write the new Exif APP1 marker segment.
	
	if ( this->exifMgr != 0 ) {

		void* exifPtr;
		XMP_Uns32 exifLen = this->exifMgr->UpdateMemoryStream ( &exifPtr );
		if ( exifLen > kExifMaxDataLength ) exifLen = this->exifMgr->UpdateMemoryStream ( &exifPtr, true );
		if ( exifLen > kExifMaxDataLength ) XMP_Throw ( "Overflow of Exif APP1 data", kXMPErr_BadJPEG );
	
		if ( exifLen > 0 ) {
			first4 = MakeUns32BE ( 0xFFE10000 + 2 + kExifSignatureLength + exifLen );
			LFA_Write ( destRef, &first4, 4 );
			LFA_Write ( destRef, kExifSignatureString, kExifSignatureLength );
			LFA_Write ( destRef, exifPtr, exifLen );
		}
	
	}

	// Write the new XMP APP1 marker segment, with possible extension marker segments.
	
	std::string mainXMP, extXMP, extDigest;
	SXMPUtils::PackageForJPEG ( this->xmpObj, &mainXMP, &extXMP, &extDigest );
	XMP_Assert ( (extXMP.size() == 0) || (extDigest.size() == 32) );
	
	first4 = MakeUns32BE ( 0xFFE10000 + 2 + kMainXMPSignatureLength + mainXMP.size() );
	LFA_Write ( destRef, &first4, 4 );
	LFA_Write ( destRef, kMainXMPSignatureString, kMainXMPSignatureLength );
	LFA_Write ( destRef, mainXMP.c_str(), mainXMP.size() );
	
	size_t extPos = 0;
	size_t extLen = extXMP.size();
	
	while ( extLen > 0 ) {

		size_t partLen = extLen;
		if ( partLen > 65000 ) partLen = 65000;

		first4 = MakeUns32BE ( 0xFFE10000 + 2 + kExtXMPPrefixLength + partLen );
		LFA_Write ( destRef, &first4, 4 );
		
		LFA_Write ( destRef, kExtXMPSignatureString, kExtXMPSignatureLength );
		LFA_Write ( destRef, extDigest.c_str(), extDigest.size() );

		first4 = MakeUns32BE ( extXMP.size() );
		LFA_Write ( destRef, &first4, 4 );
		first4 = MakeUns32BE ( extPos );
		LFA_Write ( destRef, &first4, 4 );
		
		LFA_Write ( destRef, &extXMP[extPos], partLen );
		
		extPos += partLen;
		extLen -= partLen;
		
	}

	// Write the new PSIR APP13 marker segment.
	
	if ( this->psirMgr != 0 ) {

		void* psirPtr;
		XMP_Uns32 psirLen = this->psirMgr->UpdateMemoryResources ( &psirPtr );
		if ( psirLen > kPSIRMaxDataLength ) XMP_Throw ( "Overflow of PSIR APP13 data", kXMPErr_BadJPEG );
		
		if ( psirLen > 0 ) {
			first4 = MakeUns32BE ( 0xFFED0000 + 2 + kPSIRSignatureLength + psirLen );
			LFA_Write ( destRef, &first4, 4 );
			LFA_Write ( destRef, kPSIRSignatureString, kPSIRSignatureLength );
			LFA_Write ( destRef, psirPtr, psirLen );
		}
		
	}
	
	// Copy remaining marker segments, skipping old metadata, to the first SOFn marker.
	
	while ( true ) {

		if ( checkAbort && abortProc(abortArg) ) {
			XMP_Throw ( "JPEG_MetaHandler::WriteFile - User abort", kXMPErr_UserAbort );
		}
	
		if ( ! CheckFileSpace ( sourceRef, &ioBuf, 2 ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );
		marker = GetUns16BE ( ioBuf.ptr );
		if ( marker == 0xFFFF ) {
			LFA_Write ( destRef, ioBuf.ptr, 1 );	// Copy the 0xFF pad byte.
			++ioBuf.ptr;
			continue;
		}
		
		if ( ! TableOrDataMarker ( marker ) ) break;

		if ( ! CheckFileSpace ( sourceRef, &ioBuf, 4 ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );
		segLen = GetUns16BE ( ioBuf.ptr+2 );
	
		if ( ! CheckFileSpace ( sourceRef, &ioBuf, 2+segLen ) ) XMP_Throw ( "Unexpected end to JPEG", kXMPErr_BadJPEG );

		bool copySegment = true;
		XMP_Uns8* signaturePtr = ioBuf.ptr + 4;
		
		if ( marker == 0xFFED ) {
			if ( (segLen >= kPSIRSignatureLength) &&
				 CheckBytes ( signaturePtr, kPSIRSignatureString, kPSIRSignatureLength ) ) {
				copySegment = false;
			}
		} else if ( marker == 0xFFE1 ) {
			if ( (segLen >= kExifSignatureLength) &&
				 (CheckBytes ( signaturePtr, kExifSignatureString, kExifSignatureLength ) ||
				  CheckBytes ( signaturePtr, kExifSignatureAltStr, kExifSignatureLength )) ) {
				copySegment = false;
			} else if ( (segLen >= kMainXMPSignatureLength) &&
						CheckBytes ( signaturePtr, kMainXMPSignatureString, kMainXMPSignatureLength ) ) {
				copySegment = false;
			} else if ( (segLen >= kExtXMPPrefixLength) &&
				 		CheckBytes ( signaturePtr, kExtXMPSignatureString, kExtXMPSignatureLength ) ) {
				copySegment = false;
			}
		}

		if ( copySegment ) LFA_Write ( destRef, ioBuf.ptr, 2+segLen );

		ioBuf.ptr += 2+segLen;
		
	}
	
	// Copy the remainder of the source file.
	
	size_t bufTail = ioBuf.len - (ioBuf.ptr - &ioBuf.data[0]);
	LFA_Write ( destRef, ioBuf.ptr, bufTail );
	ioBuf.ptr += bufTail;
	
	while ( true ) {
		RefillBuffer ( sourceRef, &ioBuf );
		if ( ioBuf.len == 0 ) break;
		LFA_Write ( destRef, ioBuf.ptr, ioBuf.len );
		ioBuf.ptr += ioBuf.len;
	}
	
	this->needsUpdate = false;

}	// JPEG_MetaHandler::WriteFile
