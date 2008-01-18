using System;

namespace SE.Halligang.CsXmpToolkit
{
	public enum FileFormat : int
	{
        Pdf			= 0x50444620,
        /// <summary>
        /// PS, general PostScript following DSC conventions.
        /// </summary>
        PostScript	= 0x50532020,
        /// <summary>
        /// Encapsulated PostScript.
        /// </summary>
        Eps			= 0x45505320,

        Jpeg		= 0x4A504547,
        /// <summary>
        /// JPX, ISO 15444-1
        /// </summary>
        Jpeg2k		= 0x4A505820,
		Tiff		= 0x54494646,
		Gif			= 0x47494620,
		Png			= 0x504E4720,

		Swf			= 0x53574620,
		Fla			= 0x464C4120,
		Flv			= 0x464C5620,

		/// <summary>
		/// Quicktime
		/// </summary>
		Mov			= 0x4D4F5620,
		Avi			= 0x41564920,
		/// <summary>
		/// Cineon
		/// </summary>
		Cin			= 0x43494E20,
		Wav			= 0x57415620,
		Mp3			= 0x4D503320,
		/// <summary>
		/// Audition session
		/// </summary>
	    Ses			= 0x53455320,
		/// <summary>
		/// Audition loop
		/// </summary>
		Cel			= 0x43454C20,
		Mpeg		= 0x4D504547,
		/// <summary>
		/// MP2
		/// </summary>
		Mpeg2		= 0x4D503220,
		/// <summary>
		/// MP4, ISO 14494-12 and -14
		/// </summary>
		Mpeg4		= 0x4D503420,
		/// <summary>
		/// Windows Media Audio and Video
		/// </summary>
		Wmav		= 0x574D4156,
		Aiff		= 0x41494646,

		Html		= 0x48544D4C,
		Xml			= 0x584D4C20,
		/// <summary>
		/// Text
		/// </summary>
		Xmp			= 0x74657874,

		/// <summary>
		/// PSD
		/// </summary>
		Photoshop	= 0x50534420,
		/// <summary>
		/// AI
		/// </summary>
	    Illustrator	= 0x41492020,
		/// <summary>
		/// INDD
		/// </summary>
		InDesign	= 0x494E4444,
		/// <summary>
		/// AEP
		/// </summary>
	    AEProject	= 0x41455020,
		/// <summary>
		/// AET, After Effects Project Template
		/// </summary>
		AEProjTemplate = 0x41455420,
		/// <summary>
		/// FFX
		/// </summary>
		AEFilterPreset = 0x46465820,
		/// <summary>
		/// NCOR
		/// </summary>
		EncoreProject = 0x4E434F52,
		/// <summary>
		/// PRPJ
		/// </summary>
		PremiereProject = 0x5052504A,
		/// <summary>
		/// PRTL
		/// </summary>
		PremiereTitle = 0x5052544C,

		Unknown		= 0x20202020,
	}
}
