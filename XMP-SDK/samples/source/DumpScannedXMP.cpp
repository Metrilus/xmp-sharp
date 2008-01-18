// =================================================================================================
// Copyright 2002-2005 Adobe Systems Incorporated
// All Rights Reserved.
//
// NOTICE:  Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include <string>
#include <time.h>

#include <stdio.h>
#include <stdlib.h>
#include <stdexcept>
#include <errno.h>

#if WIN_ENV
	#pragma warning ( disable : 4127 )	// conditional expression is constant
	#pragma warning ( disable : 4996 )	// '...' was declared deprecated
#endif

#define TXMP_STRING_TYPE	std::string

#include "XMP.hpp"
#include "XMP.incl_cpp"

#include "XMPScanner.hpp"

using namespace std;

// =================================================================================================

static XMP_Status DumpCallback ( void * refCon, XMP_StringPtr outStr, XMP_StringLen outLen )
{
	XMP_Status	status	= 0; 
	size_t		count;
	FILE *		outFile	= static_cast < FILE * > ( refCon );
	
	count = fwrite ( outStr, 1, outLen, outFile );
	if ( count != outLen ) status = errno;
	return status;
	
}	// DumpCallback

// =================================================================================================

static void
ProcessPacket ( const char * fileName,
				FILE *       inFile,
			    size_t		 offset,
			    size_t       length )
{
	std::string xmlString;
	xmlString.append ( length, ' ' );
	fseek ( inFile, offset, SEEK_SET );
	fread ( (void*)xmlString.data(), 1, length, inFile );
	
	char title [1000];
	
	sprintf ( title, "// Dumping raw input for \"%s\" (%d..%d)", fileName, offset, (offset + length - 1) );
	printf ( "// " );
	for ( size_t i = 3; i < strlen(title); ++i ) printf ( "=" );
	printf ( "\n\n%s\n\n%.*s\n\n", title, length, xmlString.c_str() );
	fflush ( stdout );
	
	SXMPMeta xmpObj;
	try {
		xmpObj.ParseFromBuffer ( xmlString.c_str(), length );
	} catch ( ... ) {
		printf ( "## Parse failed\n\n" );
		return;
	}
	
	xmpObj.DumpObject ( DumpCallback, stdout );
	fflush ( stdout );
	
	string xmpString;
	xmpObj.SerializeToBuffer ( &xmpString, kXMP_OmitPacketWrapper );
	printf ( "\nPretty serialization, %d bytes :\n\n%s\n", xmpString.size(), xmpString.c_str() );
	fflush ( stdout );

	xmpObj.SerializeToBuffer ( &xmpString, (kXMP_OmitPacketWrapper | kXMP_UseCompactFormat) );
	printf ( "Compact serialization, %d bytes :\n\n%s\n", xmpString.size(), xmpString.c_str() );
	fflush ( stdout );

}	// ProcessPacket

// =================================================================================================

static void
ProcessFile ( const char * fileName  )
{
	FILE * inFile;
	size_t fileLen, readCount;
	size_t snipCount;
	char buffer [64*1024];

	// ---------------------------------------------------------------------
	// Use the scanner to find all of the packets then process each of them.
	
	inFile = fopen ( fileName, "rb" );
	if ( inFile == 0 ) {
		printf ( "Can't open \"%s\"\n", fileName );
		return;
	}
	
	fseek ( inFile, 0, SEEK_END );
	fileLen = ftell ( inFile );	// ! Only handles up to 2GB files.
	fseek ( inFile, 0, SEEK_SET );

	XMPScanner scanner ( fileLen );
	
	for ( size_t filePos = 0; true; filePos += readCount ) {
		readCount = fread ( buffer, 1, sizeof(buffer), inFile );
		if ( readCount == 0 ) break;
		scanner.Scan ( buffer, filePos, readCount );
	}
	
	snipCount = scanner.GetSnipCount();
	
	XMPScanner::SnipInfoVector snips (snipCount);
	scanner.Report ( snips );

	size_t packetCount = 0;
	for ( size_t s = 0; s < snipCount; ++s ) {
		if ( snips[s].fState == XMPScanner::eValidPacketSnip ) {
			++packetCount;
			ProcessPacket ( fileName, inFile, (size_t)snips[s].fOffset, (size_t)snips[s].fLength );
		}
	}
	if ( packetCount == 0 ) printf ( "   No packets found\n" );

}	// ProcessFile

// =================================================================================================

extern "C" int
main ( int argc, const char * argv [] )
{

	if ( ! SXMPMeta::Initialize() ) {
		printf ( "## SXMPMeta::Initialize failed!\n" );
		return -1;
	}	

	if ( argc > 1 ) {
	
		printf ( "\n" );
		for ( int i = 1; i < argc; i++ ) ProcessFile ( argv[i] );
	
	} else {

		char fileNameBuffer[1025];

		while ( true ) {
		
			printf ( "\nFile: " );
			fgets( fileNameBuffer, sizeof(fileNameBuffer), stdin );
			string	fileName ( fileNameBuffer );

			if ( fileName.empty() ) break;

			if ( (fileName[fileName.size()-1] == '\n') || (fileName[fileName.size()-1] == '\r') ) {
				fileName.erase ( fileName.size()-1, 1 );	// Remove eol, allowing for CRLF.
				if ( (fileName[fileName.size()-1] == '\n') || (fileName[fileName.size()-1] == '\r') ) {
					fileName.erase ( fileName.size()-1, 1 );
				}
			}

			if ( fileName == "." ) break;
			
			// Dragging an icon on Windows pastes a quoted path.
			if ( fileName[fileName.size()-1] == '"' ) fileName.erase ( fileName.size()-1, 1 );
			if ( fileName[0] == '"' ) fileName.erase ( 0, 1 );

			if ( ! fileName.empty() ) {
				printf ( "\n" );
				ProcessFile ( fileName.c_str() );
			}
		
		}

	}
		
	SXMPMeta::Terminate();
	return 0;

}
