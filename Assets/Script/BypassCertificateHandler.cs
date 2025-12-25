using UnityEngine;
using UnityEngine.Networking;

// Custom certificate handler to allow insecure HTTP connections
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Always return true to allow all certificates (for local development)
        return true;
    }
}

