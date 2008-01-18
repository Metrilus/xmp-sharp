#ifndef __XMPFiles_hpp__
#define __XMPFiles_hpp__	1

// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2002-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include <string>

#define TXMP_STRING_TYPE std::string
#include "XMP.hpp"

typedef void * LFA_FileRef;
class XMPFileHandler;

// =================================================================================================
/// \file XMPFiles.hpp
/// \brief High level support to access metadata in files of interest to Adobe applications.
///
/// This header ...
///
// =================================================================================================

// =================================================================================================
// *** Usage Notes (eventually to become Doxygen comments) ***
// ===========================================================
//
// This is the main part of the internal (DLL side) implementation of XMPFiles. Other parts are
// the entry point wrappers and the file format handlers. The XMPFiles class distills the client
// API from TXMPFiles.hpp, removing convenience overloads and substituting a pointer/length pair
// for output strings.
//
// The wrapper functions provide a stable binary interface and perform minor impedance correction
// between the client template API from TDocMeta.hpp and the DLL's XMPFiles class. The details of
// the wrappers should be considered private.
//
// File handlers are registered during DLL initialization with hard coded calls in Init_XMPFiles.
// Each file handler provides 2 standalone functions, CheckFormatProc and DocMetaHandlerCTor, plus a
// class derived from DocMetaHandler. The format and capability flags are passed when registering.
// This allows the same physical handler to be registered for multiple formats.
//
// -------------------------------------------------------------------------------------------------
//
// Basic outlines of the processing by the XMPFiles methods:
//
//	Constructor:
//		- Minimal work to create an empty XMPFiles object, set the ref count to 1.
//
//	Destructor:
//		- Decrement the ref count, return if greater than zero.
//		- Call LFA_Close if necessary.
//
//	UnlockLib & UnlockObj:
//		- Release the thread lock. Same for now, no per-object lock.
//
//	GetFormatInfo:
//		- Return the flags for the registered handler.
//
//	OpenFile:
//		- The physical file is opened via LFA_OpenFile.
//		- A handler is selected by calling the registered format checkers.
//		- The handler object is created by calling the registered constructor proc.
//
//	CloseFile:
//		- Return if there is no open file (not an error).
//		- If not a crash-safe update (includes read-only or no update), or the handler owns the file:
//			- Throw an exception if the handler owns the file but does not support safe update.
//			- If the file needs updating, call the handler's UpdateFile method.
//		- else:
//			- If the handler supports file rewrite:
//				- *** This might not preserve ownership and permissions.
//				- Create an empty temp file.
//				- Call the handler's WriteFile method, writing to the temp file.
//			- else
//				- *** This preserves ownership, permissions, and Mac resources.
//				- Copy the original file to a temp name (Mac data fork only).
//				- Rename the original file to a different temp name.
//				- Rename the copy file back to the original name.
//				- Call the handler's UpdateFile method for the "original as temp" file.
//			- Close both the original and temp files.
//			- Delete the file with the original name.
//			- Rename the temp file to the original name.
//		- Delete the handler object.
//		- Call LFA_Close if necessary.
//
//	GetFileInfo:
//		- Return the file info from the XMPFiles member variables.
//
//	GetXMP:
//		- Throw an exception if there is no open file.
//		- Call the handler's GetXMP method.
//
//	PutXMP:
//		- Throw an exception if there is no open file.
//		- Call the handler's PutXMP method.
//
//	CanPutXMP:
//		- Implement roughly as shown in TXMPFiles.hpp, there is no handler CanPutXMP method.
//
// -------------------------------------------------------------------------------------------------
//
// The format checker should do nothing but the minimal work to identify the overall file format.
// In particular it should not look for XMP or other metadata. Note that the format checker has no
// means to carry state forward, it just returns a yes/no answer about a particular file format.
//
// The format checker and file handler should use the LFA_* functions for all I/O. They should not
// open or close the file themselves unless the handler sets the "handler-owns-file" flag.
//
// The format checker is passed the format being checked, allowing one checker to handle multiple
// formats. It is passed the LFA file ref so that it can do additional reads if necessary. The
// buffer is from the start of the file, the file will be positioned to the byte following the
// buffer. The buffer length will be at least 4K, unless the file is smaller in which case it will
// be the length of the file. This buffer may be reused for additional reads.
//
// Identifying some file formats can require checking variable length strings. Doing seeks and reads
// for each is suboptimal. There are utilities to maintain a rolling buffer and ensure that a given
// amount of data is available. See the template file handler code for details.
//
// -------------------------------------------------------------------------------------------------
//
// The file handler has no explicit open and close methods. These are implicit in the handler's
// constructor and destructor. The file handler should use the XMPFiles member variables for the
// active file ref (and path if necessary), unless it owns the file. Note that these might change
// between the open and close in the case of crash-safe updates. Don't copy the XMPFiles member
// variables in the handler's constructor, save the pointer to the XMPFiles object and access
// directly as needed.
//
// The handler should have an UpdateFile method. This is called from XMPFiles::CloseFile if the
// file needs to be updated. The handler's destructor must only close the file, not update it.
// The handler can optionally have a WriteFile method, if it can rewrite the entire file.
//
// The handler is free to use its best judgement about caching parts of the file in memory. Overall
// speed of a single open/get/put/close cycle is probably the best goal, assuming a modern processor
// with a reasonable (significant but not enormous) amount of RAM.
//
// The handler methods will be called in a per-object thread safe manner. Concurrent access might
// occur for different objects, but not for the same object. The handler's constructor and destructor
// will always be globally serialized, so they can safely modify global data structures.
//
// (Testing issue: What about separate XMPFiles objects accessing the same file?)
//
// Handler's must not have any global objects that are heap allocated. Use pointers to objects that
// are allocated and deleted during the XMPFiles initialization and termination process. Some
// client apps are very picky about what they detect as memory leaks.
//
//    static char gSomeBuffer [10*1000];          // OK, not from the heap.
//    static std::string gSomeString;             // Not OK, content from the heap.
//    static std::vector<int> gSomeVector;        // Not OK, content from the heap.
//    static std::string * gSomeString = 0;       // OK, alloc at init, delete at term.
//    static std::vector<int> * gSomeVector = 0;  // OK, alloc at init, delete at term.
//
// =================================================================================================

class XMPFiles {
public:

	static void GetVersionInfo ( XMP_VersionInfo * info );
    
    static bool Initialize ( XMP_OptionBits options = 0 );
    static void Terminate();
    
	XMPFiles();
	virtual ~XMPFiles();

	static void UnlockLib();
	void UnlockObj();

	static bool GetFormatInfo ( XMP_FileFormat   format,
                                XMP_OptionBits * flags = 0 );

	bool OpenFile ( XMP_StringPtr  filePath,
			        XMP_FileFormat format = kXMP_UnknownFile,
			        XMP_OptionBits openFlags = 0 );
    
	void CloseFile ( XMP_OptionBits closeFlags = 0 );
	
	bool GetFileInfo ( XMP_StringPtr *  filePath = 0,
                       XMP_StringLen *  filePathLen = 0,
			           XMP_OptionBits * openFlags = 0,
			           XMP_FileFormat * format = 0,
			           XMP_OptionBits * handlerFlags = 0 );
    
	void SetAbortProc ( XMP_AbortProc abortProc,
						void *        abortArg );
    
	bool GetXMP ( SXMPMeta *       xmpObj = 0,
		          XMP_StringPtr *  xmpPacket = 0,
		          XMP_StringLen *  xmpPacketLen = 0,
                  XMP_PacketInfo * packetInfo = 0 );

	bool GetThumbnail ( XMP_ThumbnailInfo * tnailInfo );
    
	void PutXMP ( const SXMPMeta & xmpObj );
    
	void PutXMP ( XMP_StringPtr xmpPacket,
                  XMP_StringLen xmpPacketLen = kXMP_UseNullTermination );
    
	bool CanPutXMP ( const SXMPMeta & xmpObj );
    
	bool CanPutXMP ( XMP_StringPtr xmpPacket,
                     XMP_StringLen xmpPacketLen = kXMP_UseNullTermination );
	
	// Leave this data public so file handlers can see it.

	XMP_Int32 clientRefs;	// ! Must be signed to allow decrement from zero.

	XMP_FileFormat   format;
	LFA_FileRef      fileRef;		// Non-zero if a file is open.
	std::string      filePath;
	XMP_OptionBits   openFlags;
	XMPFileHandler * handler;		// Non-null if a file is open.
	void *           handlerTemp;	// For use between the CheckProc and handler creation.

	XMP_AbortProc    abortProc;
	void *           abortArg;

};	// XMPFiles

#endif /* __XMPFiles_hpp__ */
