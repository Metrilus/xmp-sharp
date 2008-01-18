#ifndef __InDesign_Handler_hpp__
#define __InDesign_Handler_hpp__	1

// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2002-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "Basic_Handler.hpp"

// =================================================================================================
/// \file InDesign_Handler.hpp
/// \brief File format handler for InDesign files.
///
/// This header ...
///
// =================================================================================================

extern XMPFileHandler * InDesign_MetaHandlerCTor ( XMPFiles * parent );

extern bool InDesign_CheckFormat ( XMP_FileFormat format,
								   XMP_StringPtr  filePath,
			                       LFA_FileRef    fileRef,
			                       XMPFiles *     parent );

static const XMP_OptionBits kInDesign_HandlerFlags = kBasic_HandlerFlags & (~kXMPFiles_CanInjectXMP);	// ! InDesign can't inject.

class InDesign_MetaHandler : public Basic_MetaHandler
{
public:

	InDesign_MetaHandler ( XMPFiles * parent );
	~InDesign_MetaHandler();

	void CacheFileData();

protected:
	
	void WriteXMPPrefix();
	void WriteXMPSuffix();
	
	void NoteXMPRemoval();
	void NoteXMPInsertion();
	
	void CaptureFileEnding();
	void RestoreFileEnding();
	
	bool streamBigEndian;	// Set from master page's fObjectStreamEndian.
	XMP_Uns32 xmpObjID;		// Set from contiguous object's fObjectID, still as little endian.
	XMP_Uns32 xmpClassID;	// Set from contiguous object's fObjectClassID, still as little endian.
	
};	// InDesign_MetaHandler

// =================================================================================================

#endif /* __InDesign_Handler_hpp__ */
