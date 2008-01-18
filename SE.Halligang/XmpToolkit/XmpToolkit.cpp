// XmpToolkit.cpp : Defines the entry point for the DLL application.
//

#include <string>
#include <windows.h>
using namespace std;

#define TXMP_STRING_TYPE std::string
#define XMP_INCLUDE_XMPFILES 1
#include "XMP.incl_cpp"

int __stdcall DllMain( void* hModule,
                       unsigned long ul_reason_for_call,
                       void* lpReserved
					 )
{
    return 1;
}

#define DllExport __declspec( dllexport )

// XMP Files
extern "C"
{
	// *************************************************************************
	// XMPFiles
	// *************************************************************************
	// -------------------------------------------------------------------------
	// Constructor and destructor
	// -------------------------------------------------------------------------
	DllExport SXMPFiles* XMPFiles_Construct1()
	{
		SXMPFiles* pXmpFiles = new SXMPFiles();
		return pXmpFiles;
	}

	DllExport SXMPFiles* XMPFiles_Construct2(XMP_StringPtr filePath, XMP_FileFormat format, XMP_OptionBits openFlags)
	{
		SXMPFiles* pXmpFiles = new SXMPFiles(filePath, format, openFlags);
		return pXmpFiles;
	}

	DllExport void XMPFiles_Destruct(SXMPFiles* pXmpFiles)
	{
		delete pXmpFiles;
	}

	// -------------------------------------------------------------------------
	// Public member functions
	// -------------------------------------------------------------------------
	// OpenFile, CloseFile, and related file-oriented operations
	// .........................................................................
	DllExport bool XMPFiles_OpenFile(SXMPFiles* pXmpFiles, XMP_StringPtr filePath, XMP_FileFormat format, XMP_OptionBits openFlags)
	{
		return pXmpFiles->OpenFile(filePath, format, openFlags);
	}

	DllExport void XMPFiles_CloseFile(SXMPFiles* pXmpFiles, XMP_OptionBits closeFlags)
	{
		pXmpFiles->CloseFile(closeFlags);
	}

	DllExport bool XMPFiles_GetFileInfo(SXMPFiles* pXmpFiles, XMP_StringPtr filePath, XMP_OptionBits* openFlags, XMP_FileFormat* format, XMP_OptionBits* handlerFlags)
	{
		return pXmpFiles->GetFileInfo(0, openFlags, format, handlerFlags);
	}

	DllExport void XMPFiles_SetAbortProc(SXMPFiles* pXmpFiles, XMP_AbortProc abortProc, void* abortArg)
	{
		pXmpFiles->SetAbortProc(abortProc, abortArg);
	}

	// Metadata Access Functions
	// .........................................................................
	DllExport bool XMPFiles_GetXMP(SXMPFiles* pXmpFiles, SXMPMeta* xmpObj, bool getXmpPacket, XMP_StringPtr* xmpPacket, XMP_Uns32* xmpPacketLength, XMP_PacketInfo* packetInfo)
	{
		if (getXmpPacket)
		{
			if (xmpPacket == NULL || xmpPacketLength == NULL)
			{
				return false;
			}

			std::string tmpXmpPacket;
			if (pXmpFiles->GetXMP(xmpObj, &tmpXmpPacket, packetInfo))
			{
				*xmpPacketLength = tmpXmpPacket.length();
				try
				{
					*xmpPacket = NULL;
					*xmpPacket = (XMP_StringPtr)malloc(*xmpPacketLength);
					memcpy((void*)*xmpPacket, tmpXmpPacket.c_str(), *xmpPacketLength);
					return true;
				}
				catch ( ... )
				{
					if (xmpPacketLength != NULL)
					{
						*xmpPacketLength = 0;
						if (*xmpPacket != NULL)
						{
							delete *xmpPacket;
							*xmpPacket = NULL;
						}
					}
				}
			}
			return false;
		}
		else
		{
			return pXmpFiles->GetXMP(xmpObj, 0, packetInfo);
		}
	}

	DllExport bool XMPFiles_GetThumbnail(SXMPFiles* pXmpFiles, XMP_ThumbnailInfo* tnailInfo)
	{
		return pXmpFiles->GetThumbnail(tnailInfo);
	}

	DllExport void XMPFiles_PutXMP(SXMPFiles* pXmpFiles, SXMPMeta* xmpObj)
	{
		pXmpFiles->PutXMP(*xmpObj);
	}

	DllExport bool XMPFiles_CanPutXMP(SXMPFiles* pXmpFiles, SXMPMeta* xmpObj)
	{
		return pXmpFiles->CanPutXMP(*xmpObj);
	}

	// -------------------------------------------------------------------------
	// Static Public Member Functions
	// -------------------------------------------------------------------------
	// Initialization and termination
	// .........................................................................
	DllExport void XMPFiles_GetVersionInfo(XMP_VersionInfo* pVersionInfo)
	{
		SXMPFiles::GetVersionInfo(pVersionInfo);
	}

	DllExport bool XMPFiles_Initialize1()
	{
		return SXMPFiles::Initialize();
	}

	DllExport bool XMPFiles_Initialize2(XMP_OptionBits optionBits)
	{
		return SXMPFiles::Initialize(optionBits);
	}

	DllExport void XMPFiles_Terminate()
	{
		SXMPFiles::Terminate();
	}

	// Static Functions
	// .........................................................................
	DllExport bool XMPFiles_GetFormatInfo(XMP_FileFormat format, XMP_OptionBits* handlerFlags)
	{
		return SXMPFiles::GetFormatInfo(format, handlerFlags);
	}
}

// XMPMeta
extern "C"
{
	// *************************************************************************
	// XMPMeta
	// *************************************************************************
	// -------------------------------------------------------------------------
	// Constructor and destructor
	// -------------------------------------------------------------------------
	DllExport SXMPMeta* XMPMeta_Construct1()
	{
		SXMPMeta* pXmpMeta = new SXMPMeta();
		return pXmpMeta;
	}

	DllExport SXMPMeta* XMPMeta_Construct2(XMP_StringPtr buffer, XMP_StringLen xmpSize)
	{
		SXMPMeta* pXmpMeta = new SXMPMeta(buffer, xmpSize);
		return pXmpMeta;
	}

	DllExport void XMPMeta_Destruct(SXMPMeta* pXmpMeta)
	{
		delete pXmpMeta;
	}

	// -------------------------------------------------------------------------
	// Public Member Functions
	// -------------------------------------------------------------------------
	// Functions for getting property values
	// .........................................................................
	DllExport bool XMPMeta_GetProperty(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr* propValue, XMP_Uns32* propValueLength, XMP_OptionBits* options)
	{
		if (propValueLength == NULL)
		{
			return pXmpMeta->GetProperty(schemaNS, propName, NULL, options);
		}
		else
		{
			*propValueLength = 0;
			std::string tmpPropValue;
			if (pXmpMeta->GetProperty(schemaNS, propName, &tmpPropValue, options))
			{
				*propValueLength = tmpPropValue.length();
				try
				{
					*propValue = NULL;
					*propValue = (XMP_StringPtr)malloc(*propValueLength);
					memcpy((void*)*propValue, (void*)tmpPropValue.c_str(), *propValueLength);
					return true;
				}
				catch ( ... )
				{
					if (propValueLength != NULL)
					{
						*propValueLength = 0;
						if (*propValue != NULL)
						{
							delete *propValue;
							*propValue = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport bool XMPMeta_GetArrayItem(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_Index itemIndex, XMP_StringPtr* itemValue, XMP_Uns32* itemValueLength, XMP_OptionBits* options)
	{
		if (itemValueLength == NULL)
		{
			return pXmpMeta->GetArrayItem(schemaNS, arrayName, itemIndex, NULL, options);
		}
		else
		{
			*itemValueLength = 0;
			std::string tmpItemValue;
			if (pXmpMeta->GetArrayItem(schemaNS, arrayName, itemIndex, &tmpItemValue, options))
			{
				*itemValueLength = tmpItemValue.length();
				try
				{
					*itemValue = NULL;
					*itemValue = (XMP_StringPtr)malloc(*itemValueLength);
					memcpy((void*)*itemValue, (void*)tmpItemValue.c_str(), *itemValueLength);
					return true;
				}
				catch ( ... )
				{
					if (itemValueLength != NULL)
					{
						*itemValueLength = 0;
						if (*itemValue != NULL)
						{
							delete *itemValue;
							*itemValue = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport bool XMPMeta_GetStructField(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr structName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName, XMP_StringPtr* fieldValue, XMP_Uns32* fieldValueLength, XMP_OptionBits* options)
	{
		if (fieldValueLength == NULL)
		{
			return pXmpMeta->GetStructField(schemaNS, structName, fieldNS, fieldName, NULL, options);
		}
		else
		{
			*fieldValueLength = 0;
			std::string tmpFieldValue;
			if (pXmpMeta->GetStructField(schemaNS, structName, fieldNS, fieldName, &tmpFieldValue, options))
			{
				*fieldValueLength = tmpFieldValue.length();
				try
				{
					*fieldValue = NULL;
					*fieldValue = (XMP_StringPtr)malloc(*fieldValueLength);
					memcpy((void*)*fieldValue, (void*)tmpFieldValue.c_str(), *fieldValueLength);
					return true;
				}
				catch ( ... )
				{
					if (fieldValueLength != NULL)
					{
						*fieldValueLength = 0;
						if (*fieldValue != NULL)
						{
							delete *fieldValue;
							*fieldValue = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport bool XMPMeta_GetQualifier(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr qualNS, XMP_StringPtr qualName, XMP_StringPtr* qualValue, XMP_Uns32* qualValueLength, XMP_OptionBits* options)
	{
		if (qualValueLength == NULL)
		{
			return pXmpMeta->GetQualifier(schemaNS, propName, qualNS, qualName, NULL, options);
		}
		else
		{
			*qualValueLength = 0;
			std::string tmpQualValue;
			if (pXmpMeta->GetQualifier(schemaNS, propName, qualNS, qualName, &tmpQualValue, options))
			{
				*qualValueLength = tmpQualValue.length();
				try
				{
					*qualValue = NULL;
					*qualValue = (XMP_StringPtr)malloc(*qualValueLength);
					memcpy((void*)*qualValue, (void*)tmpQualValue.c_str(), *qualValueLength);
					return true;
				}
				catch ( ... )
				{
					if (qualValueLength != NULL)
					{
						*qualValueLength = 0;
						if (*qualValue != NULL)
						{
							delete *qualValue;
							*qualValue = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	// Functions for setting property values
	// .........................................................................
	DllExport void XMPMeta_SetProperty(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetArrayItem(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_Index itemIndex, XMP_StringPtr itemValue, XMP_OptionBits options)
	{
		pXmpMeta->SetArrayItem(schemaNS, arrayName, itemIndex, itemValue, options);
	}

	DllExport void XMPMeta_AppendArrayItem(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_OptionBits arrayOptions, XMP_StringPtr itemValue, XMP_OptionBits itemOptions)
	{
		pXmpMeta->AppendArrayItem(schemaNS, arrayName, arrayOptions, itemValue, itemOptions);
	}

	DllExport void XMPMeta_SetStructField(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr structName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName, XMP_StringPtr fieldValue, XMP_OptionBits options)
	{
		pXmpMeta->SetStructField(schemaNS, structName, fieldNS, fieldName, fieldValue, options);
	}

	DllExport void XMPMeta_SetQualifier(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr qualNS, XMP_StringPtr qualName, XMP_StringPtr qualValue, XMP_OptionBits options)
	{
		pXmpMeta->SetQualifier(schemaNS, propName, qualNS, qualName, qualValue, options);
	}

	// Functions for deleting and detecting properties
	// .........................................................................
	DllExport void XMPMeta_DeleteProperty(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName)
	{
		pXmpMeta->DeleteProperty(schemaNS, propName);
	}

	DllExport void XMPMeta_DeleteArrayItem(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_Index itemIndex)
	{
		pXmpMeta->DeleteArrayItem(schemaNS, arrayName, itemIndex);
	}

	DllExport void XMPMeta_DeleteStructField(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr structName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName)
	{
		pXmpMeta->DeleteStructField(schemaNS, structName, fieldNS, fieldName);
	}

	DllExport void XMPMeta_DeleteQualifier(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr qualNS, XMP_StringPtr qualName)
	{
		pXmpMeta->DeleteQualifier(schemaNS, propName, qualNS, qualName);
	}

	DllExport bool XMPMeta_DoesPropertyExist(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName)
	{
		return pXmpMeta->DoesPropertyExist(schemaNS, propName);
	}

	DllExport bool XMPMeta_DoesArrayItemExist(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_Index itemIndex)
	{
		return pXmpMeta->DoesArrayItemExist(schemaNS, arrayName, itemIndex);
	}

	DllExport bool XMPMeta_DoesStructFieldExist(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr structName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName)
	{
		return pXmpMeta->DoesStructFieldExist(schemaNS, structName, fieldNS, fieldName);
	}

	DllExport bool XMPMeta_DoesQualifierExist(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr qualNS, XMP_StringPtr qualName)
	{
		return pXmpMeta->DoesQualifierExist(schemaNS, propName, qualNS, qualName);
	}

	// Functions for accessing localized text (alt-text) properties
	// .........................................................................
	DllExport bool XMPMeta_GetLocalizedText(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr altTextName, XMP_StringPtr genericLang, XMP_StringPtr specificLang, XMP_StringPtr* actualLang, XMP_Uns32* actualLangLength, XMP_StringPtr* itemValue, XMP_Uns32* itemValueLength, XMP_OptionBits* options)
	{
		if (actualLangLength == NULL && itemValueLength == NULL)
		{
			return pXmpMeta->GetLocalizedText(schemaNS, altTextName, genericLang, specificLang, NULL, NULL, options);
		}
		else if (itemValueLength == NULL)
		{
			*actualLangLength = 0;
			std::string tmpActualLang;
			if (pXmpMeta->GetLocalizedText(schemaNS, altTextName, genericLang, specificLang, &tmpActualLang, NULL, options))
			{
				*actualLangLength = tmpActualLang.length();
				try
				{
					*actualLang = NULL;
					*actualLang = (XMP_StringPtr)malloc(*actualLangLength);
					memcpy((void*)*actualLang, (void*)tmpActualLang.c_str(), *actualLangLength);
					return true;
				}
				catch ( ... )
				{
					if (actualLangLength != NULL)
					{
						*actualLangLength = 0;
						if (*actualLang != NULL)
						{
							delete *actualLang;
							*actualLang = NULL;
						}
					}

					throw;
				}
			}
		}
		else if (actualLangLength == NULL)
		{
			*itemValueLength = 0;
			std::string tmpItemValue;
			if (pXmpMeta->GetLocalizedText(schemaNS, altTextName, genericLang, specificLang, NULL, &tmpItemValue, options))
			{
				*itemValueLength = tmpItemValue.length();
				try
				{
					*itemValue = NULL;
					*itemValue = (XMP_StringPtr)malloc(*itemValueLength);
					memcpy((void*)*itemValue, (void*)tmpItemValue.c_str(), *itemValueLength);
					return true;
				}
				catch ( ... )
				{
					if (itemValueLength != NULL)
					{
						*itemValueLength = 0;
						if (*itemValue != NULL)
						{
							delete *itemValue;
							*itemValue = NULL;
						}
					}

					throw;
				}
			}
		}
		else
		{
			*actualLangLength = 0;
			*itemValueLength = 0;
			std::string tmpActualLang;
			std::string tmpItemValue;
			if (pXmpMeta->GetLocalizedText(schemaNS, altTextName, genericLang, specificLang, &tmpActualLang, &tmpItemValue, options))
			{
				*actualLangLength = tmpActualLang.length();
				*itemValueLength = tmpItemValue.length();
				try
				{
					*actualLang = NULL;
					*itemValue = NULL;
					*actualLang = (XMP_StringPtr)malloc(*actualLangLength);
					*itemValue = (XMP_StringPtr)malloc(*itemValueLength);
					memcpy((void*)*actualLang, (void*)tmpActualLang.c_str(), *actualLangLength);
					memcpy((void*)*itemValue, (void*)tmpItemValue.c_str(), *itemValueLength);
					return true;
				}
				catch ( ... )
				{
					if (actualLangLength != NULL)
					{
						*actualLangLength = 0;
						if (*actualLang != NULL)
						{
							delete *actualLang;
							*actualLang = NULL;
						}
					}

					if (itemValueLength != NULL)
					{
						*itemValueLength = 0;
						if (*itemValue != NULL)
						{
							delete *itemValue;
							*itemValue = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport void XMPMeta_SetLocalizedText(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr altTextName, XMP_StringPtr genericLang, XMP_StringPtr specificLang, XMP_StringPtr itemValue, XMP_OptionBits options)
	{
		pXmpMeta->SetLocalizedText(schemaNS, altTextName, genericLang, specificLang, itemValue, options);
	}

	// Functions accessing properties as binary values
	// .........................................................................
	DllExport bool XMPMeta_GetProperty_Bool(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, bool* propValue, XMP_OptionBits* options)
	{
		return pXmpMeta->GetProperty_Bool(schemaNS, propName, propValue, options);
	}

	DllExport bool XMPMeta_GetProperty_Int(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, long* propValue, XMP_OptionBits* options)
	{
		return pXmpMeta->GetProperty_Int(schemaNS, propName, propValue, options);
	}

	DllExport bool XMPMeta_GetProperty_Int64(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, long long* propValue, XMP_OptionBits* options)
	{
		return pXmpMeta->GetProperty_Int64(schemaNS, propName, propValue, options);
	}

	DllExport bool XMPMeta_GetProperty_Float(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, double* propValue, XMP_OptionBits* options)
	{
		return pXmpMeta->GetProperty_Float(schemaNS, propName, propValue, options);
	}

	DllExport bool XMPMeta_GetProperty_Date(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_DateTime* propValue, XMP_OptionBits* options)
	{
		return pXmpMeta->GetProperty_Date(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetProperty_Bool(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, bool propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty_Bool(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetProperty_Int(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, long propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty_Int(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetProperty_Int64(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, long long propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty_Int64(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetProperty_Float(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, double propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty_Float(schemaNS, propName, propValue, options);
	}

	DllExport void XMPMeta_SetProperty_Date(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_DateTime propValue, XMP_OptionBits options)
	{
		pXmpMeta->SetProperty_Date(schemaNS, propName, propValue, options);
	}

	// Misceallaneous functions
	// .........................................................................
	DllExport XMP_Status XMPMeta_DumpObject(SXMPMeta* pXmpMeta, XMP_TextOutputProc outProc, void *refCon)
	{
		return pXmpMeta->DumpObject(outProc, refCon);
	}

	// Functions for parsing and serializing
	// .........................................................................
	DllExport void XMPMeta_ParseFromBuffer(SXMPMeta* pXmpMeta, XMP_StringPtr buffer, XMP_StringLen bufferSize, XMP_OptionBits options)
	{
		pXmpMeta->ParseFromBuffer(buffer, bufferSize, options);
	}

	DllExport void XMPMeta_SerializeToBuffer1(SXMPMeta* pXmpMeta, XMP_StringPtr* rdfString, XMP_Uns32* rdfStringLength, XMP_OptionBits options, XMP_StringLen padding, XMP_StringPtr newline, XMP_StringPtr indent, XMP_Index baseIndent)
	{
		if (rdfStringLength != NULL)
		{
			*rdfStringLength = 0;
			std::string tmpRdfString;
			pXmpMeta->SerializeToBuffer(&tmpRdfString, options, padding, newline, indent, baseIndent);
			*rdfStringLength = tmpRdfString.length();
			try
			{
				*rdfString = NULL;
				*rdfString = (XMP_StringPtr)malloc(*rdfStringLength);
				memcpy((void*)*rdfString, (void*)tmpRdfString.c_str(), *rdfStringLength);
			}
			catch ( ... )
			{ 
				if (rdfStringLength != NULL)
				{
					*rdfStringLength = 0;
					if (*rdfString != NULL)
					{
						delete *rdfString;
						*rdfString = NULL;
					}
				}
			}
		}
	}

	DllExport void XMPMeta_SerializeToBuffer2(SXMPMeta* pXmpMeta, XMP_StringPtr* rdfString, XMP_Uns32* rdfStringLength, XMP_OptionBits options, XMP_StringLen padding)
	{
		if (rdfStringLength != NULL)
		{
			*rdfStringLength = 0;
			std::string tmpRdfString;
			pXmpMeta->SerializeToBuffer(&tmpRdfString, options, padding);
			*rdfStringLength = tmpRdfString.length();
			try
			{
				*rdfString = NULL;
				*rdfString = (XMP_StringPtr)malloc(*rdfStringLength);
				memcpy((void*)*rdfString, (void*)tmpRdfString.c_str(), *rdfStringLength);
			}
			catch ( ... )
			{ 
				if (rdfStringLength != NULL)
				{
					*rdfStringLength = 0;
					if (*rdfString != NULL)
					{
						delete *rdfString;
						*rdfString = NULL;
					}
				}
			}
		}
	}

	// -------------------------------------------------------------------------
	// Static Public Member Functions
	// -------------------------------------------------------------------------
	// Initialization and termination
	// .........................................................................
	DllExport void XMPMeta_GetVersionInfo(XMP_VersionInfo* pVersionInfo)
	{
		SXMPMeta::GetVersionInfo(pVersionInfo);
	}

	DllExport bool XMPMeta_Initialize()
	{
		return SXMPMeta::Initialize();
	}

	DllExport void XMPMeta_Terminate()
	{
		SXMPMeta::Terminate();
	}

	// Global option flags
	// .........................................................................
	DllExport XMP_OptionBits XMPMeta_GetGlobalOptions()
	{
		return SXMPMeta::GetGlobalOptions();
	}

	DllExport void SetGlobalOptions(XMP_OptionBits options)
	{
		SXMPMeta::SetGlobalOptions(options);
	}

	// Internal data structure dump utilities
	// .........................................................................
	DllExport XMP_Status XMPMeta_DumpNamespaces(XMP_TextOutputProc outProc, void* refCon)
	{
		return SXMPMeta::DumpNamespaces(outProc, refCon);
	}

	DllExport XMP_Status XMPMeta_DumpAliases(XMP_TextOutputProc outProc, void* refCon)
	{
		return SXMPMeta::DumpAliases(outProc, refCon);
	}

	// Namespace Functions
	// .........................................................................
	DllExport bool XMPMeta_RegisterNamespace(XMP_StringPtr namespaceURI, XMP_StringPtr suggestedPrefix, XMP_StringPtr* registeredPrefix, XMP_Uns32* registeredPrefixLength)
	{
		if (registeredPrefixLength != NULL)
		{
			*registeredPrefixLength = 0;
			std::string tmpRegisteredPrefix;
			if (SXMPMeta::RegisterNamespace(namespaceURI, suggestedPrefix, &tmpRegisteredPrefix))
			{
				*registeredPrefixLength = tmpRegisteredPrefix.length();
				try
				{
					*registeredPrefix = (XMP_StringPtr)malloc(*registeredPrefixLength);
					memcpy((void*)*registeredPrefix, (void*)tmpRegisteredPrefix.c_str(), *registeredPrefixLength);
					return true;
				}
				catch ( ... )
				{
					if (registeredPrefixLength != NULL)
					{
						*registeredPrefixLength = 0;
						if (*registeredPrefix != NULL)
						{
							delete *registeredPrefix;
							*registeredPrefix = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport bool XMPMeta_GetNamespacePrefix(XMP_StringPtr namespaceURI, XMP_StringPtr* namespacePrefix, XMP_Uns32* namespacePrefixLength)
	{
		if (namespacePrefixLength != NULL)
		{
			*namespacePrefixLength = 0;
			std::string tmpNamespacePrefix;
			if (SXMPMeta::GetNamespacePrefix(namespaceURI, &tmpNamespacePrefix))
			{
				*namespacePrefixLength = tmpNamespacePrefix.length();
				try
				{
					*namespacePrefix = (XMP_StringPtr)malloc(*namespacePrefixLength);
					memcpy((void*)*namespacePrefix, (void*)tmpNamespacePrefix.c_str(), *namespacePrefixLength);
					return true;
				}
				catch ( ... )
				{
					if (namespacePrefixLength != NULL)
					{
						*namespacePrefixLength = 0;
						if (*namespacePrefix != NULL)
						{
							delete *namespacePrefix;
							*namespacePrefix = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport bool XMPMeta_GetNamespaceURI(XMP_StringPtr namespacePrefix, XMP_StringPtr* namespaceURI, XMP_Uns32* namespaceURILength)
	{
		if (namespaceURILength != NULL)
		{
			*namespaceURILength = 0;
			std::string tmpNamespaceURI;
			if (SXMPMeta::GetNamespaceURI(namespacePrefix, &tmpNamespaceURI))
			{
				*namespaceURILength = tmpNamespaceURI.length();
				try
				{
					*namespaceURI = (XMP_StringPtr)malloc(*namespaceURILength);
					memcpy((void*)*namespaceURI, (void*)tmpNamespaceURI.c_str(), *namespaceURILength);
					return true;
				}
				catch ( ... )
				{
					if (namespaceURILength != NULL)
					{
						*namespaceURILength = 0;
						if (*namespaceURI != NULL)
						{
							delete *namespaceURI;
							*namespaceURI = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport void XMPMeta_DeleteNamespace(XMP_StringPtr namespaceURI)
	{
		SXMPMeta::DeleteNamespace(namespaceURI);
	}

	// Alias Functions
	// .........................................................................
	DllExport void XMPMeta_RegisterAlias(XMP_StringPtr aliasNS, XMP_StringPtr aliasProp, XMP_StringPtr actualNS, XMP_StringPtr actualProp, XMP_OptionBits arrayForm)
	{
		SXMPMeta::RegisterAlias(aliasNS, aliasProp, actualNS, actualProp, arrayForm);
	}

	DllExport bool XMPMeta_ResolveAlias(XMP_StringPtr aliasNS, XMP_StringPtr aliasProp, XMP_StringPtr* actualNS, XMP_Uns32* actualNSLength, XMP_StringPtr* actualProp, XMP_Uns32* actualPropLength, XMP_OptionBits* arrayForm)
	{
		if (actualNSLength == NULL && actualPropLength == NULL)
		{
			return SXMPMeta::ResolveAlias(aliasNS, aliasProp, NULL, NULL, arrayForm);
		}
		else if (actualPropLength == NULL)
		{
			*actualNSLength = 0;
			std::string tmpActualNS;
			if (SXMPMeta::ResolveAlias(aliasNS, aliasProp, &tmpActualNS, NULL, arrayForm))
			{
				*actualNSLength = tmpActualNS.length();
				try
				{
					*actualNS = NULL;
					*actualNS = (XMP_StringPtr)malloc(*actualNSLength);
					memcpy((void*)*actualNS, (void*)tmpActualNS.c_str(), *actualNSLength);
					return true;
				}
				catch ( ... )
				{
					if (actualNSLength != NULL)
					{
						*actualNSLength = 0;
						if (*actualNS != NULL)
						{
							delete *actualNS;
							*actualNS = NULL;
						}
					}

					throw;
				}
			}
		}
		else if (actualNSLength == NULL)
		{
			*actualPropLength = 0;
			std::string tmpActualProp;
			if (SXMPMeta::ResolveAlias(aliasNS, aliasProp, NULL, &tmpActualProp, arrayForm))
			{
				*actualPropLength = tmpActualProp.length();
				try
				{
					*actualProp = NULL;
					*actualProp = (XMP_StringPtr)malloc(*actualPropLength);
					memcpy((void*)*actualProp, (void*)tmpActualProp.c_str(), *actualPropLength);
					return true;
				}
				catch ( ... )
				{
					if (actualPropLength != NULL)
					{
						*actualPropLength = 0;
						if (*actualProp != NULL)
						{
							delete *actualProp;
							*actualProp = NULL;
						}
					}

					throw;
				}
			}
		}
		else
		{
			*actualNSLength = 0;
			*actualPropLength = 0;
			std::string tmpActualNS;
			std::string tmpActualProp;
			if (SXMPMeta::ResolveAlias(aliasNS, aliasProp, &tmpActualNS, &tmpActualProp, arrayForm))
			{
				*actualNSLength = tmpActualNS.length();
				*actualPropLength = tmpActualProp.length();
				try
				{
					*actualNS = NULL;
					*actualProp = NULL;
					*actualNS = (XMP_StringPtr)malloc(*actualNSLength);
					*actualProp = (XMP_StringPtr)malloc(*actualPropLength);
					memcpy((void*)*actualNS, (void*)tmpActualNS.c_str(), *actualNSLength);
					memcpy((void*)*actualProp, (void*)tmpActualProp.c_str(), *actualPropLength);
					return true;
				}
				catch ( ... )
				{
					if (actualNSLength != NULL)
					{
						*actualNSLength = 0;
						if (*actualNS != NULL)
						{
							delete *actualNS;
							*actualNS = NULL;
						}
					}

					if (actualPropLength != NULL)
					{
						*actualPropLength = 0;
						if (*actualProp != NULL)
						{
							delete *actualProp;
							*actualProp = NULL;
						}
					}

					throw;
				}
			}
		}
		return false;
	}

	DllExport void XMPMeta_DeleteAlias(XMP_StringPtr aliasNS, XMP_StringPtr aliasProp)
	{
		SXMPMeta::DeleteAlias(aliasNS, aliasProp);
	}

	DllExport void XMPMeta_RegisterStandardAliases(XMP_StringPtr schemaNS)
	{
		SXMPMeta::RegisterStandardAliases(schemaNS);
	}
}

// XMP Utils
extern "C"
{
	// *************************************************************************
	// XMPUtils
	// *************************************************************************
	// -------------------------------------------------------------------------
	// Path composition functions
	// -------------------------------------------------------------------------
	DllExport void XMPUtils_ComposeArrayItemPath(XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_Index itemIndex, XMP_StringPtr* fullPath, XMP_Uns32* fullPathLength)
	{
		std::string tmpFullPath;
		SXMPUtils::ComposeArrayItemPath(schemaNS, arrayName, itemIndex, &tmpFullPath);
		*fullPathLength = tmpFullPath.length();
		try
		{
			*fullPath = NULL;
			*fullPath = (XMP_StringPtr)malloc(*fullPathLength);
			memcpy((void*)*fullPath, tmpFullPath.c_str(), *fullPathLength);
		}
		catch ( ... )
		{
			*fullPathLength = 0;
			if (*fullPath != NULL)
			{
				delete *fullPath;
				*fullPath = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ComposeStructFieldPath(XMP_StringPtr schemaNS, XMP_StringPtr structName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName, XMP_StringPtr* fullPath, XMP_Uns32* fullPathLength)
	{
		std::string tmpFullPath;
		SXMPUtils::ComposeStructFieldPath(schemaNS, structName, fieldNS, fieldName, &tmpFullPath);
		*fullPathLength = tmpFullPath.length();
		try
		{
			*fullPath = NULL;
			*fullPath = (XMP_StringPtr)malloc(*fullPathLength);
			memcpy((void*)*fullPath, tmpFullPath.c_str(), *fullPathLength);
		}
		catch ( ... )

		{
			*fullPathLength = 0;
			if (*fullPath != NULL)
			{
				delete *fullPath;
				*fullPath = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ComposeQualifierPath(XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_StringPtr qualNS, XMP_StringPtr qualName, XMP_StringPtr* fullPath, XMP_Uns32* fullPathLength)
	{
		std::string tmpFullPath;
		SXMPUtils::ComposeQualifierPath(schemaNS, propName, qualNS, qualName, &tmpFullPath);
		*fullPathLength = tmpFullPath.length();
		try
		{
			*fullPath = NULL;
			*fullPath = (XMP_StringPtr)malloc(*fullPathLength);
			memcpy((void*)*fullPath, tmpFullPath.c_str(), *fullPathLength);
		}
		catch ( ... )
		{
			*fullPathLength = 0;
			if (*fullPath != NULL)
			{
				delete *fullPath;
				*fullPath = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ComposeLangSelector(XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_StringPtr langName, XMP_StringPtr* fullPath, XMP_Uns32* fullPathLength)
	{
		std::string tmpFullPath;
		SXMPUtils::ComposeLangSelector(schemaNS, arrayName, langName, &tmpFullPath);
		*fullPathLength = tmpFullPath.length();
		try
		{
			*fullPath = NULL;
			*fullPath = (XMP_StringPtr)malloc(*fullPathLength);
			memcpy((void*)*fullPath, tmpFullPath.c_str(), *fullPathLength);
		}
		catch ( ... )
		{
			*fullPathLength = 0;
			if (*fullPath != NULL)
			{
				delete *fullPath;
				*fullPath = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ComposeFieldSelector(XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_StringPtr fieldNS, XMP_StringPtr fieldName, XMP_StringPtr fieldValue, XMP_StringPtr* fullPath, XMP_Uns32* fullPathLength)
	{
		std::string tmpFullPath;
		SXMPUtils::ComposeFieldSelector(schemaNS, arrayName, fieldNS, fieldName, fieldValue, &tmpFullPath);
		*fullPathLength = tmpFullPath.length();
		try
		{
			*fullPath = NULL;
			*fullPath = (XMP_StringPtr)malloc(*fullPathLength);
			memcpy((void*)*fullPath, tmpFullPath.c_str(), *fullPathLength);
		}
		catch (...)
		{
			*fullPathLength = 0;
			if (*fullPath != NULL)
			{
				delete *fullPath;
				*fullPath = NULL;
			}

			throw;
		}
	}

	// -------------------------------------------------------------------------
	// Binary-String conversion functions
	// -------------------------------------------------------------------------

	DllExport void XMPUtils_ConvertFromBool(bool binValue, XMP_StringPtr* strValue, XMP_Uns32* strValueLength)
	{
		std::string tmpStrValue;
		SXMPUtils::ConvertFromBool(binValue, &tmpStrValue);
		*strValueLength = tmpStrValue.length();
		try
		{
			*strValue = NULL;
			*strValue = (XMP_StringPtr)malloc(*strValueLength);
			memcpy((void*)*strValue, tmpStrValue.c_str(), *strValueLength);
		}
		catch ( ... )
		{
			*strValueLength = 0;
			if (*strValue != NULL)
			{
				delete *strValue;
				*strValue = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ConvertFromInt(long binValue, XMP_StringPtr format, XMP_StringPtr* strValue, XMP_Uns32* strValueLength)
	{
		std::string tmpStrValue;
		SXMPUtils::ConvertFromInt(binValue, format, &tmpStrValue);
		*strValueLength = tmpStrValue.length();
		try
		{
			*strValue = NULL;
			*strValue = (XMP_StringPtr)malloc(*strValueLength);
			memcpy((void*)*strValue, tmpStrValue.c_str(), *strValueLength);
		}
		catch ( ... )
		{
			*strValueLength = 0;
			if (*strValue != NULL)
			{
				delete *strValue;
				*strValue = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ConvertFromFloat(double binValue, XMP_StringPtr format, XMP_StringPtr* strValue, XMP_Uns32* strValueLength)
	{
		std::string tmpStrValue;
		SXMPUtils::ConvertFromFloat(binValue, format, &tmpStrValue);
		*strValueLength = tmpStrValue.length();
		try
		{
			*strValue = NULL;
			*strValue = (XMP_StringPtr)malloc(*strValueLength);
			memcpy((void*)*strValue, tmpStrValue.c_str(), *strValueLength);
		}
		catch ( ... )
		{
			*strValueLength = 0;
			if (*strValue != NULL)
			{
				delete *strValue;
				*strValue = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_ConvertFromDate(XMP_DateTime binValue, XMP_StringPtr* strValue, XMP_Uns32* strValueLength)
	{
		std::string tmpStrValue;
		SXMPUtils::ConvertFromDate(binValue, &tmpStrValue);
		*strValueLength = tmpStrValue.length();
		try
		{
			*strValue = NULL;
			*strValue = (XMP_StringPtr)malloc(*strValueLength);
			memcpy((void*)*strValue, tmpStrValue.c_str(), *strValueLength);
		}
		catch ( ... )
		{
			*strValueLength = 0;
			if (*strValue != NULL)
			{
				delete *strValue;
				*strValue = NULL;
			}

			throw;
		}
	}

	DllExport bool XMPUtils_ConvertToBool(XMP_StringPtr strValue)
	{
		return SXMPUtils::ConvertToBool(strValue);
	}

	DllExport long XMPUtils_ConvertToInt(XMP_StringPtr strValue)
	{
		return SXMPUtils::ConvertToInt(strValue);
	}

	DllExport long long XMPUtils_ConvertToInt64(XMP_StringPtr strValue)
	{
		return SXMPUtils::ConvertToInt64(strValue);
	}

	DllExport double XMPUtils_ConvertToFloat(XMP_StringPtr strValue)
	{
		return SXMPUtils::ConvertToFloat(strValue);
	}

	DllExport void XMPUtils_ConvertToDate(XMP_StringPtr strValue, XMP_DateTime* binValue)
	{
		SXMPUtils::ConvertToDate(strValue, binValue);
	}

	// -------------------------------------------------------------------------
	// Date/Time functions
	// -------------------------------------------------------------------------
	DllExport void XMPUtils_CurrentDateTime(XMP_DateTime* time)
	{
		SXMPUtils::CurrentDateTime(time);
	}

	DllExport void XMPUtils_SetTimeZone(XMP_DateTime* time)
	{
		SXMPUtils::SetTimeZone(time);
	}

	DllExport void XMPUtils_ConvertToUTCTime(XMP_DateTime* time)
	{
		SXMPUtils::ConvertToUTCTime(time);
	}

	DllExport void XMPUtils_ConvertToLocalTime(XMP_DateTime* time)
	{
		SXMPUtils::ConvertToLocalTime(time);
	}

	DllExport int XMPUtils_CompareDateTime(XMP_DateTime left, XMP_DateTime right)
	{
		return SXMPUtils::CompareDateTime(left, right);
	}

	// -------------------------------------------------------------------------
	// Base 64 Encoding and Decoding
	// -------------------------------------------------------------------------

	DllExport void XMPUtils_EncodeToBase64(XMP_StringPtr rawStr, XMP_StringLen rawLen, XMP_StringPtr* encodedStr, XMP_Uns32* encodedStrLength)
	{
		std::string tmpEncodedStr;
		SXMPUtils::EncodeToBase64(rawStr, rawLen, &tmpEncodedStr);
		*encodedStrLength = tmpEncodedStr.length();
		try
		{
			*encodedStr = NULL;
			*encodedStr = (XMP_StringPtr)malloc(*encodedStrLength);
			memcpy((void*)*encodedStr, tmpEncodedStr.c_str(), *encodedStrLength);
		}
		catch ( ... )
		{
			*encodedStrLength = 0;
			if (*encodedStr != NULL)
			{
				delete *encodedStr;
				*encodedStr = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_DecodeFromBase64(XMP_StringPtr encodedStr, XMP_StringLen encodedLen, XMP_StringPtr* rawStr, XMP_Uns32* rawStrLength)
	{
		std::string tmpRawStr;
		SXMPUtils::DecodeFromBase64(encodedStr, encodedLen, &tmpRawStr);
		*rawStrLength = tmpRawStr.length();
		try
		{
			*rawStr = NULL;
			*rawStr = (XMP_StringPtr)malloc(*rawStrLength);
			memcpy((void*)*rawStr, tmpRawStr.c_str(), *rawStrLength);
		}
		catch ( ... )
		{
			*rawStrLength = 0;
			if (*rawStr != NULL)
			{
				delete *rawStr;
				*rawStr = NULL;
			}

			throw;
		}
	}

	// -------------------------------------------------------------------------
	// JPEG file handling
	// -------------------------------------------------------------------------

	DllExport void XMPUtils_PackageForJPEG(SXMPMeta* xmpObj, XMP_StringPtr* standardXMP, XMP_Uns32* standardXMPLength, XMP_StringPtr* extendedXMP, XMP_Uns32* extendedXMPLength, XMP_StringPtr* extendedDigest, XMP_Uns32* extendedDigestLength)
	{
		std::string tmpStandardXMP;
		std::string tmpExtendedXMP;
		std::string tmpExtendedDigest;
		SXMPUtils::PackageForJPEG(*xmpObj, &tmpStandardXMP, &tmpExtendedXMP, &tmpExtendedDigest);
		try
		{
			*standardXMP = NULL;
			*extendedXMP = NULL;
			*extendedDigest = NULL;

			*standardXMPLength = tmpStandardXMP.length();
			*standardXMP = (XMP_StringPtr)malloc(*standardXMPLength);
			memcpy((void*)*standardXMP, tmpStandardXMP.c_str(), *standardXMPLength);

			*extendedXMPLength = tmpExtendedXMP.length();
			*extendedXMP = (XMP_StringPtr)malloc(*extendedXMPLength);
			memcpy((void*)*extendedXMP, tmpExtendedXMP.c_str(), *extendedXMPLength);

			*extendedDigestLength = tmpExtendedDigest.length();
			*extendedDigest = (XMP_StringPtr)malloc(*extendedDigestLength);
			memcpy((void*)*extendedDigest, tmpExtendedDigest.c_str(), *extendedDigestLength);
		}
		catch ( ... )
		{
			*standardXMPLength = 0;
			if (*standardXMP != NULL)
			{
				delete *standardXMP;
				*standardXMP = NULL;
			}
		
			*extendedXMPLength = 0;
			if (*extendedXMP != NULL)
			{
				delete *extendedXMP;
				*extendedXMP = NULL;
			}
			
			*extendedDigestLength = 0;
			if (*extendedDigest != NULL)
			{
				delete *extendedDigest;
				*extendedDigest = NULL;
			}

			throw;
		}
	}

	DllExport void XMPUtils_MergeFromJPEG(SXMPMeta* fullXMP, SXMPMeta* extendedXMP)
	{
		SXMPUtils::MergeFromJPEG(fullXMP, *extendedXMP);
	}

	// -------------------------------------------------------------------------
	// UI helper functions
	// -------------------------------------------------------------------------

	DllExport void XMPUtils_CatenateArrayItems(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_StringPtr separator, XMP_StringPtr quotes, XMP_OptionBits options, XMP_StringPtr* catedStr, XMP_Uns32* catedStrLength)
	{
		std::string tmpCatedStr;
		SXMPUtils::CatenateArrayItems(*pXmpMeta, schemaNS, arrayName, separator, quotes, options, &tmpCatedStr);
		*catedStrLength = tmpCatedStr.length();
		try
		{
			*catedStr = NULL;
			*catedStr = (XMP_StringPtr)malloc(*catedStrLength);
			memcpy((void*)*catedStr, tmpCatedStr.c_str(), *catedStrLength);
		}
		catch ( ... )
		{
			*catedStrLength = 0;
			if (*catedStr != NULL)
			{
				delete *catedStr;
				*catedStr = NULL;
			}

			throw;
		}		
	}

	DllExport void XMPUtils_SeparateArrayItems(SXMPMeta* xmpObj, XMP_StringPtr schemaNS, XMP_StringPtr arrayName, XMP_OptionBits options, XMP_StringPtr catedStr)
	{
		SXMPUtils::SeparateArrayItems(xmpObj, schemaNS, arrayName, options, catedStr);
	}

	DllExport void XMPUtils_RemoveProperties(SXMPMeta* xmpObj, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_OptionBits options)
	{
		SXMPUtils::RemoveProperties(xmpObj, schemaNS, propName, options);
	}

	DllExport void XMPUtils_AppendProperties(SXMPMeta* source, SXMPMeta* dest, XMP_OptionBits options)
	{
		SXMPUtils::AppendProperties(*source, dest, options);
	}

	DllExport void XMPUtils_DuplicateSubtree(SXMPMeta* &source, SXMPMeta* dest, XMP_StringPtr sourceNS, XMP_StringPtr sourceRoot, XMP_StringPtr destNS, XMP_StringPtr destRoot, XMP_OptionBits options)
	{
		SXMPUtils::DuplicateSubtree(*source, dest, sourceNS, sourceRoot, destNS, destRoot, options);
	}
}

// XMP Iterator
extern "C"
{
	// *************************************************************************
	// XMPIterator
	// *************************************************************************
	// -------------------------------------------------------------------------
	// Constructor and destructor
	// -------------------------------------------------------------------------
	DllExport SXMPIterator* XMPIterator_Construct(SXMPMeta* pXmpMeta, XMP_StringPtr schemaNS, XMP_StringPtr propName, XMP_OptionBits options)
	{
		SXMPIterator* pXmpIterator = new SXMPIterator(*pXmpMeta, schemaNS, propName, options);
		return pXmpIterator;
	}

	DllExport void XMPIterator_Destruct(SXMPIterator* pXmpIterator)
	{
		delete pXmpIterator;
	}

	// -------------------------------------------------------------------------
	// Public Member Functions
	// -------------------------------------------------------------------------
	DllExport bool XMPIterator_Next(SXMPIterator* pXmpIterator, XMP_StringPtr* schemaNS, XMP_Uns32* schemaNSLength,
		XMP_StringPtr* propPath, XMP_Uns32* propPathLength, XMP_StringPtr* propValue, XMP_Uns32* propValueLength, XMP_OptionBits* options)
	{
		std::string tmpSchemaNS;
		std::string tmpPropPath;
		std::string tmpPropValue;

		if (pXmpIterator->Next(
			(schemaNSLength != NULL ? &tmpSchemaNS : NULL),
			(propPathLength != NULL ? &tmpPropPath : NULL),
			(propValueLength != NULL ? &tmpPropValue : NULL),
			options))
		{
			try
			{
				*schemaNS = NULL;
				*propPath = NULL;
				*propValue = NULL;

				if (schemaNSLength != NULL && schemaNS != NULL)
				{
					*schemaNSLength = tmpSchemaNS.length();
					*schemaNS = (XMP_StringPtr)malloc(*schemaNSLength);
					memcpy((void*)*schemaNS, tmpSchemaNS.c_str(), *schemaNSLength);
				}
				if (propPathLength != NULL && propPath != NULL)
				{
					*propPathLength = tmpPropPath.length();
					*propPath = (XMP_StringPtr)malloc(*propPathLength);
					memcpy((void*)*propPath, tmpPropPath.c_str(), *propPathLength);
				}
				if (propValueLength != NULL && propValue != NULL)
				{
					*propValueLength = tmpPropValue.length();
					*propValue = (XMP_StringPtr)malloc(*propValueLength);
					memcpy((void*)*propValue, tmpPropValue.c_str(), *propValueLength);
				}
				return true;
			}
			catch ( ... )
			{
				if (schemaNSLength != NULL)
				{
					*schemaNSLength = 0;
					if (*schemaNS != NULL)
					{
						delete *schemaNS;
						*schemaNS = NULL;
					}
				}
				
				if (propPathLength != NULL)
				{
					*propPathLength = 0;
					if (*propPath != NULL)
					{
						delete *propPath;
						*propPath = NULL;
					}
				}
				
				if (propValueLength != NULL)
				{
					*propValueLength = 0;
					if (*propValue != NULL)
					{
						delete *propValue;
						*propValue = NULL;
					}
				}

				throw;
			}
		}
		return false;
	}

	DllExport void XMPIterator_Skip(SXMPIterator* pXmpIterator, XMP_OptionBits options)
	{
		pXmpIterator->Skip(options);
	}
}

// Common
extern "C"
{
	// *************************************************************************
	// Common
	// *************************************************************************
	DllExport void Common_FreeString(XMP_StringPtr pString)
	{
		if (pString != NULL)
		{
			try
			{
				delete pString;
				pString = NULL;
			}
			catch ( ... ) { }
		}
	}

	DllExport DWORD Common_GetLastError()
	{
		DWORD lastError = -1;
		try
		{
			lastError = ::GetLastError();
		}
		catch ( ... ) { }
		return lastError;
	}
}