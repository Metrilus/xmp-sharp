// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2002-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "InDesign_Handler.hpp"

using namespace std;

// =================================================================================================
/// \file InDesign_Handler.cpp
/// \brief File format handler for InDesign files.
///
/// This header ...
///
/// The layout of an InDesign file in terms of the Basic_MetaHandler model is:
///
/// \li The front of the file. This is everything up to the XMP contiguous object section. The file
/// starts with a pair of master pages, followed by the data pages, followed by contiguous object
/// sections, finished with padding to a page boundary.
///
/// \li A prefix for the XMP section. This is the contiguous object header. The offset is
/// (this->packetInfo.offset - this->xmpPrefixSize).
///
/// \li The XMP packet. The offset is this->packetInfo.offset.
///
/// \li A suffix for the XMP section. This is the contiguous object header. The offset is
/// (this->packetInfo.offset + this->packetInfo.length).
///
/// \li Trailing file content. This is the contiguous objects that follow the XMP. The offset is
/// (this->packetInfo.offset + this->packetInfo.length + this->xmpSuffixSize).
///
/// \li The back of the file. This is the final padding to a page boundary. The offset is
/// (this->packetInfo.offset + this->packetInfo.length + this->xmpSuffixSize + this->trailingContentSize).
///
// =================================================================================================

// *** Add PutXMP overrides that throw if the file does not contain XMP.

#ifndef TraceInDesignHandler
	#define TraceInDesignHandler 0
#endif

enum { kInDesignGUIDSize = 16 };

struct InDesignMasterPage {
	XMP_Uns8  fGUID [kInDesignGUIDSize];
	XMP_Uns8  fMagicBytes [8];
	XMP_Uns8  fObjectStreamEndian;
	XMP_Uns8  fIrrelevant1 [239];
	XMP_Uns64 fSequenceNumber;
	XMP_Uns8  fIrrelevant2 [8];
	XMP_Uns32 fFilePages;
	XMP_Uns8  fIrrelevant3 [3812];
};

enum {
	kINDD_PageSize     = 4096,
	kINDD_PageMask     = (kINDD_PageSize - 1),
	kINDD_LittleEndian = 1,
	kINDD_BigEndian    = 2 };

struct InDesignContigObjMarker {
	XMP_Uns8  fGUID [kInDesignGUIDSize];
	XMP_Uns32 fObjectUID;
	XMP_Uns32 fObjectClassID;
	XMP_Uns32 fStreamLength;
	XMP_Uns32 fChecksum;
};

static const XMP_Uns8 * kINDD_MasterPageGUID =
	(const XMP_Uns8 *) "\x06\x06\xED\xF5\xD8\x1D\x46\xE5\xBD\x31\xEF\xE7\xFE\x74\xB7\x1D";

static const XMP_Uns8 * kINDDContigObjHeaderGUID =
	(const XMP_Uns8 *) "\xDE\x39\x39\x79\x51\x88\x4B\x6C\x8E\x63\xEE\xF8\xAE\xE0\xDD\x38";

static const XMP_Uns8 * kINDDContigObjTrailerGUID =
	(const XMP_Uns8 *) "\xFD\xCE\xDB\x70\xF7\x86\x4B\x4F\xA4\xD3\xC7\x28\xB3\x41\x71\x06";

// =================================================================================================
// InDesign_MetaHandlerCTor
// ========================

XMPFileHandler * InDesign_MetaHandlerCTor ( XMPFiles * parent )
{
	return new InDesign_MetaHandler ( parent );

}	// InDesign_MetaHandlerCTor

// =================================================================================================
// InDesign_CheckFormat
// ====================
//
// For InDesign we check that the pair of master pages begin with the 16 byte GUID.

bool InDesign_CheckFormat ( XMP_FileFormat format,
	                        XMP_StringPtr  filePath,
	                        LFA_FileRef    fileRef,
	                        XMPFiles *     parent )
{
	IgnoreParam(format); IgnoreParam(filePath); IgnoreParam(parent);
	XMP_Assert ( format == kXMP_InDesignFile );
	XMP_Assert ( strlen ( (const char *) kINDD_MasterPageGUID ) == kInDesignGUIDSize );

	enum { kBufferSize = 2*kINDD_PageSize };
	XMP_Uns8 buffer [kBufferSize];

	XMP_Int64 filePos   = 0;
	XMP_Uns8 * bufPtr   = buffer;
	XMP_Uns8 * bufLimit = bufPtr + kBufferSize;

	LFA_Seek ( fileRef, 0, SEEK_SET );
	size_t bufLen = LFA_Read ( fileRef, buffer, kBufferSize );
	if ( bufLen != kBufferSize ) return false;
	
	if ( ! CheckBytes ( bufPtr, kINDD_MasterPageGUID, kInDesignGUIDSize ) ) return false;
	if ( ! CheckBytes ( bufPtr+kINDD_PageSize, kINDD_MasterPageGUID, kInDesignGUIDSize ) ) return false;

	return true;
	
}	// InDesign_CheckFormat

// =================================================================================================
// InDesign_MetaHandler::InDesign_MetaHandler
// ==========================================

InDesign_MetaHandler::InDesign_MetaHandler ( XMPFiles * _parent ) : streamBigEndian(0), xmpObjID(0), xmpClassID(0)
{
	this->parent = _parent;
	this->handlerFlags = kInDesign_HandlerFlags;
	this->stdCharForm  = kXMP_Char8Bit;

}	// InDesign_MetaHandler::InDesign_MetaHandler

// =================================================================================================
// InDesign_MetaHandler::~InDesign_MetaHandler
// ===========================================

InDesign_MetaHandler::~InDesign_MetaHandler()
{
	// Nothing to do here.
	
}	// InDesign_MetaHandler::~InDesign_MetaHandler

// =================================================================================================
// InDesign_MetaHandler::CacheFileData
// ===================================
//
// Look for the XMP in an InDesign database file. This is a paged database using 4K byte pages,
// followed by redundant "contiguous object streams". Each contiguous object stream is a copy of a
// database object stored as a contiguous byte stream. The XMP that we want is one of these.
//
// The first 2 pages of the database are alternating master pages. A generation number is used to
// select the active master page. The master page contains an offset to the start of the contiguous
// object streams. Each of the contiguous object streams contains a header and trailer, allowing
// fast motion from one stream to the next.
//
// There is no unique "what am I" tagging to the contiguous object streams, so we simply pick the
// first one that looks right. At present this is a 4 byte little endian packet size followed by the
// packet.

// ! Note that insertion of XMP is not allowed for InDesign, the XMP must be a contiguous copy of an
// ! internal database object. So we don't set the packet offset to an insertion point if not found.

void InDesign_MetaHandler::CacheFileData()
{
	LFA_FileRef fileRef = this->parent->fileRef;
	XMP_PacketInfo & packetInfo = this->packetInfo;

	IOBuffer ioBuf;
	size_t	 dbPages;
	XMP_Uns8 cobjEndian;
	
	XMP_AbortProc abortProc  = this->parent->abortProc;
	void *        abortArg   = this->parent->abortArg;
	const bool    checkAbort = (abortProc != 0);
	
	XMP_Assert ( kINDD_PageSize == sizeof(InDesignMasterPage) );
	XMP_Assert ( kIOBufferSize >= (2 * kINDD_PageSize) );
	
	this->containsXMP = false;
	
	// ---------------------------------------------------------------------------------
	// Figure out which master page is active and seek to the contiguous object portion.
	
	{
		ioBuf.filePos = 0;
		ioBuf.ptr = ioBuf.limit;	// Make sure RefillBuffer does a simple read.
		LFA_Seek ( fileRef, ioBuf.filePos, SEEK_SET );
		RefillBuffer ( fileRef, &ioBuf );
		if ( ioBuf.len < (2 * kINDD_PageSize) ) XMP_Throw ( "GetMainPacket/ScanInDesignFile: Read failure", kXMPErr_ExternalFailure );

		InDesignMasterPage * masters = (InDesignMasterPage *) ioBuf.ptr;
		XMP_Uns64 seq0 = GetUns64LE ( (XMP_Uns8 *) &masters[0].fSequenceNumber );
		XMP_Uns64 seq1 = GetUns64LE ( (XMP_Uns8 *) &masters[1].fSequenceNumber );
		
		dbPages = GetUns32LE ( (XMP_Uns8 *) &masters[0].fFilePages );
		cobjEndian = masters[0].fObjectStreamEndian;
		if ( seq1 > seq0 ) {
			dbPages = GetUns32LE ( (XMP_Uns8 *)  &masters[1].fFilePages );
			cobjEndian = masters[1].fObjectStreamEndian;
		}
	}
	
	XMP_Assert ( ! this->streamBigEndian );
	if ( cobjEndian == kINDD_BigEndian ) this->streamBigEndian = true;
	
	// ---------------------------------------------------------------------------------------------
	// Look for the XMP contiguous object stream. Most of the time there will be just one stream and
	// it will be the XMP. So we might as well fill the whole buffer and not worry about reading too
	// much and seeking back to the start of the following stream.
	
	XMP_Int64 cobjPos = (XMP_Int64)dbPages * kINDD_PageSize;	// ! Use a 64 bit multiply!
	XMP_Uns32 streamLength = (XMP_Uns32)(-(long)(2*sizeof(InDesignContigObjMarker)));	// ! For the first pass in the loop.

	while ( true ) {

		if ( checkAbort && abortProc(abortArg) ) {
			XMP_Throw ( "InDesign_MetaHandler::LocateXMP - User abort", kXMPErr_UserAbort );
		}

		// Fetch the start of the next stream and check the contiguous object header.
		// ! The writeable bit of fObjectClassID is ignored, we use the packet trailer flag.
		
		cobjPos += streamLength + (2 * sizeof(InDesignContigObjMarker));
		ioBuf.filePos = cobjPos;
		ioBuf.ptr = ioBuf.limit;	// Make sure RefillBuffer does a simple read.
		LFA_Seek ( fileRef, ioBuf.filePos, SEEK_SET );
		RefillBuffer ( fileRef, &ioBuf );
		if ( ioBuf.len < (2 * sizeof(InDesignContigObjMarker)) ) break;	// Too small, must be end of file.
		
		const InDesignContigObjMarker * cobjHeader = (const InDesignContigObjMarker *) ioBuf.ptr;
		if ( ! CheckBytes ( Uns8Ptr(&cobjHeader->fGUID), kINDDContigObjHeaderGUID, kInDesignGUIDSize ) ) break;	// Not a contiguous object header.
		this->xmpObjID = cobjHeader->fObjectUID;	// Save these now while the buffer is good.
		this->xmpClassID = cobjHeader->fObjectClassID;
		streamLength = GetUns32LE ( (XMP_Uns8 *) &cobjHeader->fStreamLength );
		ioBuf.ptr += sizeof ( InDesignContigObjMarker );
		
		// See if this is the XMP stream. Only check for UTF-8, others get caught in fallback scanning.
		
		if ( ! CheckFileSpace ( fileRef, &ioBuf, 4 ) ) continue;	// Too small, can't possibly be XMP.
		
		XMP_Uns32 innerLength = GetUns32LE ( ioBuf.ptr );
		if ( this->streamBigEndian ) innerLength = GetUns32BE ( ioBuf.ptr );
		if ( innerLength != (streamLength - 4) ) continue;	// Not legit XMP.
		ioBuf.ptr += 4;

		if ( ! CheckFileSpace ( fileRef, &ioBuf, kUTF8_PacketHeaderLen ) ) continue;	// Too small, can't possibly be XMP.
		
		if ( ! CheckBytes ( ioBuf.ptr, kUTF8_PacketStart, strlen((char*)kUTF8_PacketStart) ) ) continue;
		ioBuf.ptr += strlen((char*)kUTF8_PacketStart);

		XMP_Uns8 quote = *ioBuf.ptr;
		if ( (quote != '\'') && (quote != '"') ) continue;
		ioBuf.ptr += 1;
		if ( *ioBuf.ptr != quote ) {
			if ( ! CheckBytes ( ioBuf.ptr, Uns8Ptr("\xEF\xBB\xBF"), 3 ) ) continue;
			ioBuf.ptr += 3;
		}
		if ( *ioBuf.ptr != quote ) continue;
		ioBuf.ptr += 1;
		
		if ( ! CheckBytes ( ioBuf.ptr, Uns8Ptr(" id="), 4 ) ) continue;
		ioBuf.ptr += 4;
		quote = *ioBuf.ptr;
		if ( (quote != '\'') && (quote != '"') ) continue;
		ioBuf.ptr += 1;
		if ( ! CheckBytes ( ioBuf.ptr, kUTF8_PacketID, strlen((char*)kUTF8_PacketID) ) ) continue;
		ioBuf.ptr += strlen((char*)kUTF8_PacketID);
		if ( *ioBuf.ptr != quote ) continue;
		ioBuf.ptr += 1;
		
		// We've seen enough, it is the XMP. To fit the Basic_Handler model we need to compute the
		// total size of remaining contiguous objects, the trailingContentSize.
		
		this->xmpPrefixSize = sizeof(InDesignContigObjMarker) + 4;
		this->xmpSuffixSize = sizeof(InDesignContigObjMarker);
		packetInfo.offset = cobjPos + this->xmpPrefixSize;
		packetInfo.length = innerLength;
		
		
		XMP_Int64 tcStart = cobjPos + streamLength + (2 * sizeof(InDesignContigObjMarker));
		while ( true ) {
			if ( checkAbort && abortProc(abortArg) ) {
				XMP_Throw ( "InDesign_MetaHandler::LocateXMP - User abort", kXMPErr_UserAbort );
			}
			cobjPos += streamLength + (2 * sizeof(InDesignContigObjMarker));
			ioBuf.filePos = cobjPos;
			ioBuf.ptr = ioBuf.limit;	// Make sure RefillBuffer does a simple read.
			LFA_Seek ( fileRef, ioBuf.filePos, SEEK_SET );
			RefillBuffer ( fileRef, &ioBuf );
			if ( ioBuf.len < sizeof(InDesignContigObjMarker) ) break;	// Too small, must be end of file.
			cobjHeader = (const InDesignContigObjMarker *) ioBuf.ptr;
			if ( ! CheckBytes ( Uns8Ptr(&cobjHeader->fGUID), kINDDContigObjHeaderGUID, kInDesignGUIDSize ) ) break;	// Not a contiguous object header.
			streamLength = GetUns32LE ( (XMP_Uns8 *) &cobjHeader->fStreamLength );
		}
		this->trailingContentSize = cobjPos - tcStart;

		#if TraceInDesignHandler
			XMP_Uns32 pktOffset = (XMP_Uns32)this->packetInfo.offset;
			printf ( "Found XMP in InDesign file, offsets:\n" );
			printf ( "  CObj head %X, XMP %X, CObj tail %X, file tail %X, padding %X\n",
					 (pktOffset - this->xmpPrefixSize), pktOffset, (pktOffset + this->packetInfo.length),
					 (pktOffset + this->packetInfo.length + this->xmpSuffixSize),
					 (pktOffset + this->packetInfo.length + this->xmpSuffixSize + (XMP_Uns32)this->trailingContentSize) );
		#endif
		
		this->containsXMP = true;
		break;
				
	}

	if ( this->containsXMP ) {
		this->xmpFileOffset = packetInfo.offset;
		this->xmpFileSize = packetInfo.length;
		ReadXMPPacket ( this );
	}

}	// InDesign_MetaHandler::CacheFileData

// =================================================================================================
// InDesign_MetaHandler::WriteXMPPrefix
// ====================================

void InDesign_MetaHandler::WriteXMPPrefix()
{
	// Write the contiguous object header and the 4 byte length of the XMP packet.

	LFA_FileRef fileRef = this->parent->fileRef;
	XMP_PacketInfo & packetInfo = this->packetInfo;
	
	InDesignContigObjMarker header;
	memcpy ( header.fGUID, kINDDContigObjHeaderGUID, sizeof(header.fGUID) );	// AUDIT: Use of dest sizeof for length is safe.
	header.fObjectUID = this->xmpObjID;
	header.fObjectClassID = this->xmpClassID;
	header.fStreamLength = MakeUns32LE ( 4 + packetInfo.length );
	header.fChecksum = (XMP_Uns32)(-1);
	LFA_Write ( fileRef, &header, sizeof(header) );
	
	XMP_Uns32 pktLength = MakeUns32LE ( packetInfo.length );
	if ( this->streamBigEndian ) pktLength = MakeUns32BE ( packetInfo.length );
	LFA_Write ( fileRef, &pktLength, sizeof(pktLength) );
	
}	// InDesign_MetaHandler::WriteXMPPrefix

// =================================================================================================
// InDesign_MetaHandler::WriteXMPSuffix
// ====================================

void InDesign_MetaHandler::WriteXMPSuffix()
{
	// Write the contiguous object trailer.

	LFA_FileRef fileRef = this->parent->fileRef;
	XMP_PacketInfo & packetInfo = this->packetInfo;
	
	InDesignContigObjMarker trailer;
	
	memcpy ( trailer.fGUID, kINDDContigObjTrailerGUID, sizeof(trailer.fGUID) );	// AUDIT: Use of dest sizeof for length is safe.
	trailer.fObjectUID = this->xmpObjID;
	trailer.fObjectClassID = this->xmpClassID;
	trailer.fStreamLength = MakeUns32LE ( 4 + packetInfo.length );
	trailer.fChecksum = (XMP_Uns32)(-1);
	
	LFA_Write ( fileRef, &trailer, sizeof(trailer) );
	
}	// InDesign_MetaHandler::WriteXMPSuffix

// =================================================================================================
// InDesign_MetaHandler::NoteXMPRemoval
// ====================================

void InDesign_MetaHandler::NoteXMPRemoval()
{
	// Nothing to do.
	
}	// InDesign_MetaHandler::NoteXMPRemoval

// =================================================================================================
// InDesign_MetaHandler::NoteXMPInsertion
// ======================================

void InDesign_MetaHandler::NoteXMPInsertion()
{
	// Nothing to do.
	
}	// InDesign_MetaHandler::NoteXMPInsertion

// =================================================================================================
// InDesign_MetaHandler::CaptureFileEnding
// =======================================

void InDesign_MetaHandler::CaptureFileEnding()
{
	// Nothing to do. The back of an InDesign file is the final zero padding.

}	// InDesign_MetaHandler::CaptureFileEnding

// =================================================================================================
// InDesign_MetaHandler::RestoreFileEnding
// =======================================

void InDesign_MetaHandler::RestoreFileEnding()
{
	// Pad the file with zeros to a page boundary.

	LFA_FileRef fileRef = this->parent->fileRef;
	XMP_PacketInfo & packetInfo = this->packetInfo;
	
	XMP_Int64 dataLength = LFA_Measure ( fileRef );
	XMP_Int32 padLength  = (kINDD_PageSize - ((XMP_Int32)dataLength & kINDD_PageMask)) & kINDD_PageMask;
	
	XMP_Uns8 buffer [kINDD_PageSize];
	memset ( buffer, 0, kINDD_PageSize );
	LFA_Write ( fileRef, buffer, padLength );
		
}	// InDesign_MetaHandler::RestoreFileEnding

// =================================================================================================
