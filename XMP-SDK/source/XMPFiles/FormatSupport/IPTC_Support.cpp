// =================================================================================================
// ADOBE SYSTEMS INCORPORATED
// Copyright 2006-2007 Adobe Systems Incorporated
// All Rights Reserved
//
// NOTICE: Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

#include "IPTC_Support.hpp"
#include "EndianUtils.hpp"
#include "Reconcile_Impl.hpp"

// =================================================================================================
/// \file IPTC_Support.cpp
/// \brief XMPFiles support for IPTC (IIM) DataSets.
///
// =================================================================================================

const DataSetCharacteristics kKnownDataSets[] =
	{ { kIPTC_ObjectType,        kIPTC_UnmappedText,   67, "",                "" },						// Not mapped to XMP.
	  { kIPTC_IntellectualGenre, kIPTC_MapSpecial,     68, kXMP_NS_IPTCCore,  "IntellectualGenre" },	// Only the name part is in the XMP.
	  { kIPTC_Title,             kIPTC_MapLangAlt,     64, kXMP_NS_DC,        "title" },
	  { kIPTC_EditStatus,        kIPTC_UnmappedText,   64, "",                "" },						// Not mapped to XMP.
	  { kIPTC_EditorialUpdate,   kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP.
	  { kIPTC_Urgency,           kIPTC_MapSimple,       1, kXMP_NS_Photoshop, "Urgency" },
	  { kIPTC_SubjectCode,       kIPTC_MapSpecial,    236, kXMP_NS_IPTCCore,  "SubjectCode" },			// Only the reference number is in the XMP.
	  { kIPTC_Category,          kIPTC_MapSimple,       3, kXMP_NS_Photoshop, "Category" },
	  { kIPTC_SuppCategory,      kIPTC_MapArray,       32, kXMP_NS_Photoshop, "SupplementalCategories" },
	  { kIPTC_FixtureIdentifier, kIPTC_UnmappedText,   32, "",                "" },						// Not mapped to XMP.
	  { kIPTC_Keyword,           kIPTC_MapArray,       64, kXMP_NS_DC,        "subject" },
	  { kIPTC_ContentLocCode,    kIPTC_UnmappedText,    3, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ContentLocName,    kIPTC_UnmappedText,   64, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ReleaseDate,       kIPTC_UnmappedText,    8, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ReleaseTime,       kIPTC_UnmappedText,   11, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ExpDate,           kIPTC_UnmappedText,    8, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ExpTime,           kIPTC_UnmappedText,   11, "",                "" },						// Not mapped to XMP.
	  { kIPTC_Instructions,      kIPTC_MapSimple,     256, kXMP_NS_Photoshop, "Instructions" },
	  { kIPTC_ActionAdvised,     kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP.
	  { kIPTC_RefService,        kIPTC_UnmappedText,   10, "",                "" },						// Not mapped to XMP. ! Interleave 2:45, 2:47, 2:50!
	  { kIPTC_RefDate,           kIPTC_UnmappedText,    8, "",                "" },						// Not mapped to XMP. ! Interleave 2:45, 2:47, 2:50!
	  { kIPTC_RefNumber,         kIPTC_UnmappedText,    8, "",                "" },						// Not mapped to XMP. ! Interleave 2:45, 2:47, 2:50!
	  { kIPTC_DateCreated,       kIPTC_MapSpecial,      8, kXMP_NS_Photoshop, "DateCreated" },			// Combined with 2:60, TimeCreated.
	  { kIPTC_TimeCreated,       kIPTC_MapSpecial,     11, "",                "" },						// Combined with 2:55, DateCreated.
	  { kIPTC_DigitalCreateDate, kIPTC_UnmappedText,    8, "",                "" },						// Not mapped to XMP.
	  { kIPTC_DigitalCreateTime, kIPTC_UnmappedText,   11, "",                "" },						// Not mapped to XMP.
	  { kIPTC_OriginProgram,     kIPTC_UnmappedText,   32, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ProgramVersion,    kIPTC_UnmappedText,   10, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ObjectCycle,       kIPTC_UnmappedText,    1, "",                "" },						// Not mapped to XMP.
	  { kIPTC_Creator,           kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "Author" },				// ! Aliased to dc:creator[1].
	  { kIPTC_CreatorJobtitle,   kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "AuthorsPosition" },
	  { kIPTC_City,              kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "City" },
	  { kIPTC_Location,          kIPTC_MapSimple,      32, kXMP_NS_IPTCCore,  "Location" },
	  { kIPTC_State,             kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "State" },
	  { kIPTC_CountryCode,       kIPTC_MapSimple,       3, kXMP_NS_IPTCCore,  "CountryCode" },
	  { kIPTC_Country,           kIPTC_MapSimple,      64, kXMP_NS_Photoshop, "Country" },
	  { kIPTC_JobID,             kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "TransmissionReference" },
	  { kIPTC_Headline,          kIPTC_MapSimple,     256, kXMP_NS_Photoshop, "Headline" },
	  { kIPTC_Provider,          kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "Credit" },
	  { kIPTC_Source,            kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "Source" },
	  { kIPTC_CopyrightNotice,   kIPTC_MapLangAlt,    128, kXMP_NS_DC,        "rights" },
	  { kIPTC_Contact,           kIPTC_UnmappedText,  128, "",                "" },						// Not mapped to XMP.
	  { kIPTC_Description,       kIPTC_MapLangAlt,   2000, kXMP_NS_DC,        "description" },
	  { kIPTC_DescriptionWriter, kIPTC_MapSimple,      32, kXMP_NS_Photoshop, "CaptionWriter" },
	  { kIPTC_RasterizedCaption, kIPTC_UnmappedBin,  7360, "",                "" },						// Not mapped to XMP. ! Binary data!
	  { kIPTC_ImageType,         kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP.
	  { kIPTC_ImageOrientation,  kIPTC_UnmappedText,    1, "",                "" },						// Not mapped to XMP.
	  { kIPTC_LanguageID,        kIPTC_UnmappedText,    3, "",                "" },						// Not mapped to XMP.
	  { kIPTC_AudioType,         kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP.
	  { kIPTC_AudioSampleRate,   kIPTC_UnmappedText,    6, "",                "" },						// Not mapped to XMP.
	  { kIPTC_AudioSampleRes,    kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP.
	  { kIPTC_AudioDuration,     kIPTC_UnmappedText,    6, "",                "" },						// Not mapped to XMP.
	  { kIPTC_AudioOutcue,       kIPTC_UnmappedText,   64, "",                "" },						// Not mapped to XMP.
	  { kIPTC_PreviewFormat,     kIPTC_UnmappedBin,     2, "",                "" },						// Not mapped to XMP. ! Binary data!
	  { kIPTC_PreviewFormatVers, kIPTC_UnmappedText,    2, "",                "" },						// Not mapped to XMP. ! Binary data!
	  { kIPTC_PreviewData,       kIPTC_UnmappedText, 256000, "",              "" },						// Not mapped to XMP. ! Binary data!
	  { 255,                     kIPTC_MapSpecial,      0, 0,                 0 } };					// ! Must be last as a sentinel.

// A combination of the IPTC "Subject Reference System Guidelines" and IIMv4.1 Appendix G.
const IntellectualGenreMapping kIntellectualGenreMappings[] =
{ { "001", "Current" },
  { "002", "Analysis" },
  { "003", "Archive material" },
  { "004", "Background" },
  { "005", "Feature" },
  { "006", "Forecast" },
  { "007", "History" },
  { "008", "Obituary" },
  { "009", "Opinion" },
  { "010", "Polls and surveys" },
  { "010", "Polls & Surveys" },
  { "011", "Profile" },
  { "012", "Results listings and statistics" },
  { "012", "Results Listings & Tables" },
  { "013", "Side bar and supporting information" },
  { "013", "Side bar & Supporting information" },
  { "014", "Summary" },
  { "015", "Transcript and verbatim" },
  { "015", "Transcript & Verbatim" },
  { "016", "Interview" },
  { "017", "From the scene" },
  { "017", "From the Scene" },
  { "018", "Retrospective" },
  { "019", "Synopsis" },
  { "019", "Statistics" },
  { "020", "Update" },
  { "021", "Wrapup" },
  { "021", "Wrap-up" },
  { "022", "Press release" },
  { "022", "Press Release" },
  { "023", "Quote" },
  { "024", "Press-digest" },
  { "025", "Review" },
  { "026", "Curtain raiser" },
  { "027", "Actuality" },
  { "028", "Question and answer" },
  { "029", "Music" },
  { "030", "Response to a question" },
  { "031", "Raw sound" },
  { "032", "Scener" },
  { "033", "Text only" },
  { "034", "Voicer" },
  { "035", "Fixture" },
  { 0,     0 } };	// ! Must be last as a sentinel. 
  
// =================================================================================================
// FindKnownDataSet
// ================

static const DataSetCharacteristics* FindKnownDataSet ( XMP_Uns8 id )
{
	size_t i = 0;

	while ( kKnownDataSets[i].id < id ) ++i;	// The list is short enough for a linear search.
	
	if ( kKnownDataSets[i].id != id ) return 0;
	return &kKnownDataSets[i];	

}	// FindKnownDataSet

// =================================================================================================
// IPTC_Manager::ParseMemoryDataSets
// =================================
//
// Parse the legacy IIM block, keeping information about all 2:* DataSets and size of other records.

void IPTC_Manager::ParseMemoryDataSets ( const void* data, XMP_Uns32 length, bool copyData /* = true */ )
{
	// Get rid of any existing data.

	DataSetMap::iterator dsPos = this->dataSets.begin();
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
	for ( ; dsPos != dsEnd; ++dsPos ) this->DisposeLooseValue ( dsPos->second );

	this->dataSets.clear();
	
	if ( this->ownedContent ) free ( this->iptcContent );
	this->ownedContent = false;	// Set to true later if the content is copied.
	this->iptcContent  = 0;
	this->iptcLength   = 0;

	this->changed = false;
	
	if ( length == 0 ) return;
	if ( *((XMP_Uns8*)data) != 0x1C ) XMP_Throw ( "Not valid IPTC, no leading 0x1C", kXMPErr_BadIPTC );
	
	// Allocate space for the full in-memory data and copy it.
	
	if ( length > 10*1024*1024 ) XMP_Throw ( "Outrageous length for memory-based IPTC", kXMPErr_BadIPTC );
	this->iptcLength = length;
	
	if ( ! copyData ) {
		this->iptcContent = (XMP_Uns8*)data;
	} else {
		this->iptcContent = (XMP_Uns8*) malloc(length);
		if ( this->iptcContent == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( this->iptcContent, data, length );	// AUDIT: Safe, malloc'ed length bytes above.
		this->ownedContent = true;
	}
	
	// Build the map of the 2:* DataSets.
	
	XMP_Uns8* iptcPtr   = this->iptcContent;
	XMP_Uns8* iptcEnd   = iptcPtr + length;
	XMP_Uns8* iptcLimit = iptcEnd - kMinDataSetSize;
	XMP_Uns32 dsLen;	// ! The large form can have values up to 4GB in length.
	
	this->utf8Encoding = false;
	
	bool foundRec2 = false;
	
	for ( ; iptcPtr <= iptcLimit; iptcPtr += dsLen ) {
	
		XMP_Uns8* dsPtr  = iptcPtr;
		XMP_Uns8  oneC   = *iptcPtr;
		XMP_Uns8  recNum = *(iptcPtr+1);
		XMP_Uns8  dsNum  = *(iptcPtr+2);
		
		if ( oneC != 0x1C ) break;	// No more DataSets.
		
		dsLen  = GetUns16BE ( iptcPtr+3 );	// ! Compute dsLen before any "continue", needed for loop increment!
		iptcPtr += 5;	// Advance to the data (or extended length).
		
		if ( (dsLen & 0x8000) != 0 ) {
			XMP_Assert ( dsLen <= 0xFFFF );
			XMP_Uns32 lenLen = dsLen & 0x7FFF;
			if ( iptcPtr > iptcEnd-lenLen ) break;	// Bad final DataSet. Throw instead?
			dsLen = 0;
			for ( XMP_Uns16 i = 0; i < lenLen; ++i, ++iptcPtr ) {
				dsLen = (dsLen << 8) + *iptcPtr;
			}
		}
		
		if ( iptcPtr > (iptcEnd - dsLen) ) break;	// Bad final DataSet. Throw instead?

		if ( recNum == 0 ) continue;	// Should not be a record 0. Throw instead?

		if ( recNum == 1 ) {
			if ( (dsNum == 90) && (dsLen >= 3) ) {
				if ( memcmp ( iptcPtr, "\x1B\x25\x47", 3 ) == 0 ) this->utf8Encoding = true;
			}
			continue;	// Ignore all other record 1 DataSets.
		}

		if ( recNum == 2 ) {
			if ( ! foundRec2 ) {
				foundRec2 = true;
				this->rec2Offset = dsPtr - this->iptcContent;
				this->rec2Length = this->iptcLength - this->rec2Offset;	// ! In case there are no other records.
			}
		} else {
			this->rec2Length = dsPtr - (this->iptcContent + this->rec2Offset);
			break;	// The records are in ascending order, done.
		}
		
		XMP_Assert ( recNum == 2 );
		if ( dsNum == 0 ) continue;	// Ignore 2:00 when reading.
		
		DataSetInfo dsInfo ( dsNum, dsLen, iptcPtr );
		DataSetMap::iterator dsPos = this->dataSets.find ( dsNum );
		
		bool repeatable = false;
		
		const DataSetCharacteristics* knownDS = FindKnownDataSet ( dsNum );

		if ( (knownDS == 0) || (knownDS->mapForm == kIPTC_MapArray) ) {
			repeatable = true;	// Allow repeats for unknown DataSets.
		} else if ( dsNum == kIPTC_SubjectCode ) {
			repeatable = true;
		}
		
		if ( repeatable || (dsPos == this->dataSets.end()) ) {
			DataSetMap::value_type mapValue ( dsNum, dsInfo );
			(void) this->dataSets.insert ( this->dataSets.upper_bound ( dsNum ), mapValue );
		} else {
			this->DisposeLooseValue ( dsPos->second );
			dsPos->second = dsInfo;	// Keep the last copy of illegal repeats.
		}
	
	}

}	// IPTC_Manager::ParseMemoryDataSets

// =================================================================================================
// IPTC_Manager::GetDataSet
// ========================

size_t IPTC_Manager::GetDataSet ( XMP_Uns8 id, DataSetInfo* info, size_t which /* = 0 */ ) const
{

	DataSetMap::const_iterator dsPos = this->dataSets.lower_bound ( id );
	if ( (dsPos == this->dataSets.end()) || (id != dsPos->second.id) ) return 0;
	
	size_t dsCount = this->dataSets.count ( id );
	if ( which >= dsCount ) return 0;	// Valid range for which is 0 .. count-1.
	
	if ( info != 0 ) {
		for ( size_t i = 0; i < which; ++i ) ++dsPos;	// ??? dsPos += which;
		*info = dsPos->second;
	}
	
	return dsCount;

}	// IPTC_Manager::GetDataSet

// =================================================================================================
// IPTC_Manager::GetDataSet_UTF8
// =============================
	
size_t IPTC_Manager::GetDataSet_UTF8 ( XMP_Uns8 id, std::string * utf8Str, size_t which /* = 0 */ ) const
{
	DataSetInfo dsInfo;
	size_t dsCount = GetDataSet ( id, &dsInfo, which );
	if ( dsCount == 0 ) return 0;
	
	if ( utf8Str != 0 ) {
		if ( this->utf8Encoding ) {
			utf8Str->assign ( (char*)dsInfo.dataPtr, dsInfo.dataLen );
		} else {
			ReconcileUtils::LocalToUTF8 ( dsInfo.dataPtr, dsInfo.dataLen, utf8Str );
		}
	}
	
	return dsCount;
	
}	// IPTC_Manager::GetDataSet_UTF8

// =================================================================================================
// IPTC_Manager::DisposeLooseValue
// ===============================
//
// Dispose of loose values from SetDataSet calls after the last UpdateMemoryDataSets.

// ! Don't try to make the DataSetInfo struct be self-cleaning. It is a primary public type, returned
// ! from GetDataSet. Making it self-cleaning would get into nasty assignment and pointer ownership
// ! issues, far worse than doing this explicit cleanup.

void IPTC_Manager::DisposeLooseValue ( DataSetInfo & dsInfo )
{
	if ( dsInfo.dataLen == 0 ) return;
	
	XMP_Uns8* dataBegin = this->iptcContent;
	XMP_Uns8* dataEnd   = dataBegin + this->iptcLength;
	
	if ( ((XMP_Uns8*)dsInfo.dataPtr < dataBegin) || ((XMP_Uns8*)dsInfo.dataPtr >= dataEnd) ) {
		free ( (void*) dsInfo.dataPtr );
		dsInfo.dataPtr = 0;
	}
	
}	// IPTC_Manager::DisposeLooseValue

// =================================================================================================
// =================================================================================================

// =================================================================================================
// IPTC_Writer::~IPTC_Writer
// =========================
//
// Dispose of loose values from SetDataSet calls after the last UpdateMemoryDataSets.

IPTC_Writer::~IPTC_Writer()
{
	DataSetMap::iterator dsPos = this->dataSets.begin();
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
	for ( ; dsPos != dsEnd; ++dsPos ) this->DisposeLooseValue ( dsPos->second );
	
}	// IPTC_Writer::~IPTC_Writer

// =================================================================================================
// IPTC_Writer::SetDataSet_UTF8
// ============================

void IPTC_Writer::SetDataSet_UTF8 ( XMP_Uns8 id, const void* utf8Ptr, XMP_Uns32 utf8Len, long which /* = -1 */ )
{
	const DataSetCharacteristics* knownDS = FindKnownDataSet ( id );
	if ( knownDS == 0 ) XMP_Throw ( "Can only set known IPTC DataSets", kXMPErr_InternalFailure );
	
	// Decide which character encoding to use and get a temporary pointer to the value.
	
	XMP_Uns8 * tempPtr;
	XMP_Uns32  dataLen;

	std::string localStr, rtStr;
	
	if ( this->utf8Encoding ) {
	
		// We're already using UTF-8.
		tempPtr = (XMP_Uns8*) utf8Ptr;
		dataLen = utf8Len;
	
	} else {

// *** Disable the round trip loss checking for now. We only use UTF-8 if the input had it.
	
		ReconcileUtils::UTF8ToLocal ( utf8Ptr, utf8Len, &localStr );
//		ReconcileUtils::LocalToUTF8 ( localStr.data(), localStr.size(), &rtStr );
		
//		if ( (rtStr.size() == utf8Len) && (memcmp ( rtStr.data(), utf8Ptr, utf8Len ) == 0) ) {

			// It round-tripped without loss, keep local encoding.
			tempPtr = (XMP_Uns8*) localStr.data();
			dataLen = localStr.size();

//		} else {

//			// Had round-trip loss, change to UTF-8 for all text DataSets.
//			this->ConvertToUTF8();
//			XMP_Assert ( this->utf8Encoding );
//			tempPtr = (XMP_Uns8*) utf8Ptr;
//			dataLen = utf8Len;

//		}
		
	}
	
	// Set the value for this DataSet, making a non-transient copy of the value. Respect UTF-8 character
	// boundaries when truncating. This is easy to check. If the first truncated byte has 10 in the
	// high order 2 bits then we are in the middle of a UTF-8 multi-byte character.
	// Back up to just before a byte with 11 in the high order 2 bits.

	if ( dataLen > knownDS->maxLen ) {
		dataLen = knownDS->maxLen;
		if ( this->utf8Encoding && ((tempPtr[dataLen] >> 6) == 2) ) {
			for ( ; (dataLen > 0) && ((tempPtr[dataLen] >> 6) != 3); --dataLen ) {}
		}
	}
	
	DataSetMap::iterator dsPos = this->dataSets.find ( id );
	long currCount = (long) this->dataSets.count ( id );
	
	bool repeatable = false;
		
	if ( knownDS->mapForm == kIPTC_MapArray ) {
		repeatable = true;
	} else if ( id == kIPTC_SubjectCode ) {
		repeatable = true;
	}
	
	if ( ! repeatable ) {

		if ( which > 0 ) XMP_Throw ( "Non-repeatable IPTC DataSet", kXMPErr_BadParam );

	} else {
	
		if ( which < 0 ) which = currCount;	// The default is to append.

		if ( which > currCount ) {
			XMP_Throw ( "Invalid index for IPTC DataSet", kXMPErr_BadParam );
		} else if ( which == currCount ) {
			dsPos = this->dataSets.end();	// To make later checks do the right thing.
		} else {
			dsPos = this->dataSets.lower_bound ( id );
			for ( ; which > 0; --which ) ++dsPos;
		}
	
	}

	if ( dsPos != this->dataSets.end() ) {
		if ( (dsPos->second.dataLen == dataLen) && (memcmp ( dsPos->second.dataPtr, tempPtr, dataLen ) == 0) ) {
			return;	// ! New value matches the old, don't update.
		}
	}
	
	XMP_Uns8 * dataPtr = (XMP_Uns8*) malloc ( dataLen );
	if ( dataPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
	memcpy ( dataPtr, tempPtr, dataLen );	// AUDIT: Safe, malloc'ed dataLen bytes above.
	
	DataSetInfo dsInfo ( id, dataLen, dataPtr );

	if ( dsPos != this->dataSets.end() ) {
		this->DisposeLooseValue ( dsPos->second );
		dsPos->second = dsInfo;
	} else {
		DataSetMap::value_type mapValue ( id, dsInfo );
		(void) this->dataSets.insert ( this->dataSets.upper_bound ( id ), mapValue );
	}
	
	this->changed = true;

}	// IPTC_Writer::SetDataSet_UTF8

// =================================================================================================
// IPTC_Writer::DeleteDataSet
// ==========================

void IPTC_Writer::DeleteDataSet ( XMP_Uns8 id, long which /* = -1 */ )
{
	DataSetMap::iterator dsBegin = this->dataSets.lower_bound ( id );	// Set for which == -1.
	DataSetMap::iterator dsEnd   = this->dataSets.upper_bound ( id );
	
	if ( dsBegin == dsEnd ) return;	// Nothing to delete.

	if ( which >= 0 ) {
		long currCount = (long) this->dataSets.count ( id );
		if ( which >= currCount ) return;	// Nothing to delete.
		for ( ; which > 0; --which ) ++dsBegin;
		dsEnd = dsBegin; ++dsEnd;	// ! Can't do "dsEnd = dsBegin+1"!
	}

	for ( DataSetMap::iterator dsPos = dsBegin; dsPos != dsEnd; ++dsPos ) {
		this->DisposeLooseValue ( dsPos->second );
	}

	this->dataSets.erase ( dsBegin, dsEnd );
	this->changed = true;

}	// IPTC_Writer::DeleteDataSet

// =================================================================================================
// IPTC_Writer::UpdateMemoryDataSets
// =================================
//
// Reconstruct the entire IIM block. Start with DataSet 1:0 and 1:90 if UTF-8 encoding is used,
// then 2:0, then 2:xx DataSets that have values. This does not include any alignment padding, that
// is an artifact of some specific wrappers such as Photoshop image resources.

XMP_Uns32 IPTC_Writer::UpdateMemoryDataSets ( void** dataPtr )
{
	if ( ! this->changed ) {
		if ( dataPtr != 0 ) *dataPtr = this->iptcContent;
		return this->iptcLength;
	}

	DataSetMap::iterator dsPos;
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
//	if ( this->utf8Encoding ) {		*** Disable round trip loss checking for now. ***
//		if ( ! this->CheckRoundTripLoss() ) this->ConvertToLocal();
//	}
	
	// Compute the length of the new IIM block, including space for records other than 2. All other
	// records are preserved as-is, including 1:90. If we ever start changing the encoding, we will
	// have to remove any existing 1:90 and insert a new one.

	XMP_Uns32 newLength = (5+2);	// For 2:0.
	newLength += (this->iptcLength - rec2Length);	// For records other than 2.
	
	for ( dsPos = this->dataSets.begin(); dsPos != dsEnd; ++dsPos ) {
		XMP_Uns32 dsLen = dsPos->second.dataLen;
		newLength += (5 + dsLen);
		if ( dsLen > 0x7FFF ) newLength += 4;	// We always use a 4 byte extended length.
	}
	
	// Allocate the new IIM block.
	
	XMP_Uns8* newContent = (XMP_Uns8*) malloc ( newLength );
	if ( newContent == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
	
	XMP_Uns8* dsPtr = newContent;
	
	XMP_Uns32 prefixLength = this->rec2Offset;
	XMP_Uns32 suffixOffset = this->rec2Offset + this->rec2Length;
	XMP_Uns32 suffixLength = this->iptcLength - suffixOffset;
	
	if ( prefixLength > 0 ) {	// Write the records before 2.
		memcpy ( dsPtr, this->iptcContent, prefixLength );	// AUDIT: Within range of allocation.
		dsPtr += prefixLength;
	}
	
	if ( ! this->utf8Encoding ) {
		// Start with 2:00 for version 2.
		// *** We should probably write version 4 all the time. This is a late CS3 change, don't want
		// *** to risk breaking other apps that might be strict about version checking.
		memcpy ( dsPtr, "\x1C\x02\x00\x00\x02\x00\x02", (5+2) );	// AUDIT: Within range of allocation.
		dsPtr += (5+2);
	} else {
		// Start with 2:00 for version 4.
		memcpy ( dsPtr, "\x1C\x02\x00\x00\x02\x00\x04", (5+2) );	// AUDIT: Within range of allocation.
		dsPtr += (5+2);
	}
	
	// Fill in the record 2 DataSets that have values.
	
	for ( dsPos = this->dataSets.begin(); dsPos != dsEnd; ++dsPos ) {
	
		DataSetInfo & dsInfo = dsPos->second;
	
		dsPtr[0] = 0x1C;
		dsPtr[1] = 2;
		dsPtr[2] = dsInfo.id;
		dsPtr += 3;
		
		XMP_Uns32 dsLen = dsInfo.dataLen;
		if ( dsLen <= 0x7FFF ) {
			PutUns16BE ( (XMP_Uns16)dsLen, dsPtr );
			dsPtr += 2;
		} else {
			PutUns16BE ( 0x8004, dsPtr );
			PutUns32BE ( dsLen, dsPtr+2 );
			dsPtr += 6;
		}
		
		if ( dsLen > (newLength - (dsPtr - newContent)) ) {
			XMP_Throw ( "Buffer overrun", kXMPErr_InternalFailure );
		}
		memcpy ( dsPtr, dsInfo.dataPtr, dsLen );	// AUDIT: Protected by above check.
		dsPtr += dsLen;

	}
	
	if ( suffixLength > 0 ) {	// Write the records after 2.
		memcpy ( dsPtr, (this->iptcContent + suffixOffset), suffixLength );	// AUDIT: Within range of allocation.
		dsPtr += suffixLength;
	}
	
	XMP_Assert ( dsPtr == (newContent + newLength) );
	
	// Parse the new block, it is the best way to reset internal info and rebuild the map. 
	
	this->ParseMemoryDataSets ( newContent, newLength, false );	// Don't make another copy of the content.
	XMP_Assert ( this->iptcLength == newLength );
	this->ownedContent = true;	// We really do own the new content.
	
	// Done.

	if ( dataPtr != 0 ) *dataPtr = this->iptcContent;
	return this->iptcLength;
	
}	// IPTC_Writer::UpdateMemoryDataSets

#if 0	// *** Disable the round trip loss checking for now.

// =================================================================================================
// IPTC_Writer::ConvertToUTF8
// ==========================
//
// Convert the values of existing text DataSets to UTF-8. For now we only accept text DataSets.

void IPTC_Writer::ConvertToUTF8()
{
	XMP_Assert ( ! this->utf8Encoding );
	std::string utf8Str;

	DataSetMap::iterator dsPos = this->dataSets.begin();
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
	for ( ; dsPos != dsEnd; ++dsPos ) {

		DataSetInfo & dsInfo = dsPos->second;

		ReconcileUtils::LocalToUTF8 ( dsInfo.dataPtr, dsInfo.dataLen, &utf8Str );
		this->DisposeLooseValue ( dsInfo );

		dsInfo.dataLen = utf8Str.size();
		dsInfo.dataPtr = (XMP_Uns8*) malloc ( dsInfo.dataLen );
		if ( dsInfo.dataPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( dsInfo.dataPtr, utf8Str.data(), dsInfo.dataLen );	// AUDIT: Safe, malloc'ed dataLen bytes above.

	}
	
	this->utf8Encoding = true;

}	// IPTC_Writer::ConvertToUTF8

// =================================================================================================
// IPTC_Writer::ConvertToLocal
// ===========================
//
// Convert the values of existing text DataSets to local. For now we only accept text DataSets.

void IPTC_Writer::ConvertToLocal()
{
	XMP_Assert ( this->utf8Encoding );
	std::string localStr;

	DataSetMap::iterator dsPos = this->dataSets.begin();
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
	for ( ; dsPos != dsEnd; ++dsPos ) {

		DataSetInfo & dsInfo = dsPos->second;

		ReconcileUtils::UTF8ToLocal ( dsInfo.dataPtr, dsInfo.dataLen, &localStr );
		this->DisposeLooseValue ( dsInfo );

		dsInfo.dataLen = localStr.size();
		dsInfo.dataPtr = (XMP_Uns8*) malloc ( dsInfo.dataLen );
		if ( dsInfo.dataPtr == 0 ) XMP_Throw ( "Out of memory", kXMPErr_NoMemory );
		memcpy ( dsInfo.dataPtr, localStr.data(), dsInfo.dataLen );	// AUDIT: Safe, malloc'ed dataLen bytes above.

	}
	
	this->utf8Encoding = false;

}	// IPTC_Writer::ConvertToLocal

// =================================================================================================
// IPTC_Writer::CheckRoundTripLoss
// ===============================
//
// See if we still need UTF-8 because of round-trip loss. Returns true if there is loss.

bool IPTC_Writer::CheckRoundTripLoss()
{
	XMP_Assert ( this->utf8Encoding );
	std::string localStr, rtStr;

	DataSetMap::iterator dsPos = this->dataSets.begin();
	DataSetMap::iterator dsEnd = this->dataSets.end();
	
	for ( ; dsPos != dsEnd; ++dsPos ) {

		DataSetInfo & dsInfo = dsPos->second;

		XMP_StringPtr utf8Ptr = (XMP_StringPtr) dsInfo.dataPtr;
		XMP_StringLen utf8Len = dsInfo.dataLen;

		ReconcileUtils::UTF8ToLocal ( utf8Ptr, utf8Len, &localStr );
		ReconcileUtils::LocalToUTF8 ( localStr.data(), localStr.size(), &rtStr );

		if ( (rtStr.size() != utf8Len) || (memcmp ( rtStr.data(), utf8Ptr, utf8Len ) != 0) ) {
			return true; // Had round-trip loss, keep UTF-8.
		}

	}
	
	return false;	// No loss.

}	// IPTC_Writer::CheckRoundTripLoss

#endif
