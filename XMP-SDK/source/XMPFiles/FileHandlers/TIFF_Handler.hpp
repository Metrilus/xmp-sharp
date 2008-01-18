#ifndef __TIFF_Handler_hpp__
#define __TIFF_Handler_hpp__	1

// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2002-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "TIFF_Support.hpp"
#include "PSIR_Support.hpp"
#include "IPTC_Support.hpp"

// =================================================================================================
/// \file TIFF_Handler.hpp
/// \brief File format handler for TIFF.
///
/// This header ...
///
// =================================================================================================

// *** Could derive from Basic_Handler - buffer file tail in a temp file.

extern XMPFileHandler * TIFF_MetaHandlerCTor ( XMPFiles * parent );

extern bool TIFF_CheckFormat ( XMP_FileFormat format,
							   XMP_StringPtr  filePath,
                               LFA_FileRef    fileRef,
                               XMPFiles *     parent );

static const XMP_OptionBits kTIFF_HandlerFlags = (kXMPFiles_CanInjectXMP |
                                                  kXMPFiles_CanExpand |
                                                  kXMPFiles_CanRewrite |
                                                  kXMPFiles_PrefersInPlace |
                                                  kXMPFiles_CanReconcile |
                                                  kXMPFiles_AllowsOnlyXMP |
                                                  kXMPFiles_ReturnsRawPacket |
                                                  kXMPFiles_ReturnsTNail |
                                                  kXMPFiles_AllowsSafeUpdate);

class TIFF_MetaHandler : public XMPFileHandler
{
public:

	void CacheFileData();
	void ProcessTNail();
	void ProcessXMP();
	
	void UpdateFile ( bool doSafeUpdate );
    void WriteFile  ( LFA_FileRef sourceRef, const std::string & sourcePath );

	TIFF_MetaHandler ( XMPFiles * parent );
	virtual ~TIFF_MetaHandler();

private:

	TIFF_MetaHandler() : psirMgr(0), iptcMgr(0) {};	// Hidden on purpose.
	
	TIFF_FileWriter tiffMgr;	// The TIFF part is always file-based.
	PSIR_Manager *  psirMgr;	// Need to use pointers so we can properly select between read-only and
	IPTC_Manager *  iptcMgr;	//	read-write modes of usage.
	
};	// TIFF_MetaHandler

// =================================================================================================

#endif /* __TIFF_Handler_hpp__ */
