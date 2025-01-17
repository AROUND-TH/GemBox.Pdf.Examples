using GemBox.Pdf;
using GemBox.Pdf.Forms;
using GemBox.Pdf.Security;

class Program
{
    static void Main()
    {
        AuthorSignature();

        SignatureLocks();

        SignaturesWorkflow();
    }

    static void AuthorSignature()
    {
        // If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        // Create signed document with author permission.
        using (var document = PdfDocument.Load("Reading.pdf"))
        {
            var textField = document.Form.Fields.AddText(document.Pages[0], 50, 530, 200, 20);
            textField.Name = "Field1";
            textField.Value = "Value before signing";

            var signatureField = document.Form.Fields.AddSignature(document.Pages[0], 300, 500, 250, 50);
            signatureField.Name = "Signature1";

            var digitalId = new PdfDigitalId("GemBoxECDsa521.pfx", "GemBoxPassword");
            var signer = new PdfSigner(digitalId);

            // Specify a certification signature with actions that are permitted after certifying the document.
            signer.AuthorPermission = PdfUserAccessPermissions.FillForm;

            // Adobe Acrobat Reader currently doesn't download the certificate chain
            // so we will also embed a certificate of intermediate Certificate Authority in the signature.
            // (see https://community.adobe.com/t5/acrobat/signature-validation-using-aia-extension-not-enabled-by-default/td-p/10729647)
            signer.ValidationInfo = new PdfSignatureValidationInfo(new PdfCertificate[] { new PdfCertificate("GemBoxECDsa.crt") }, null, null);

            signatureField.Sign(signer);

            document.Save("SignatureWithFillFormAccess.pdf");
        }

        // We're modifying the field's value of the signed document,
        // but the signature will remain valid because of the specified PdfUserAccessPermissions.FillForm.
        using (var document = PdfDocument.Load("SignatureWithFillFormAccess.pdf"))
        {
            var textField = (PdfTextField)document.Form.Fields["Field1"];
            textField.Value = "Value after signing";
            document.Save();
        }
    }

    static void SignatureLocks()
    {
        // If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = PdfDocument.Load("Reading.pdf"))
        {
            var textField1 = document.Form.Fields.AddText(document.Pages[0], 50, 530, 200, 20);
            textField1.Name = "Text1";
            textField1.Value = "If changed signature is invalid";

            var textField2 = document.Form.Fields.AddText(document.Pages[0], 50, 480, 200, 20);
            textField2.Name = "Text2";
            textField2.Value = "If changed signature is still valid";

            var signatureField = document.Form.Fields.AddSignature(document.Pages[0], 300, 500, 250, 50);
            signatureField.Name = "Signature1";
            signatureField.SetLockedFields(textField1);

            var digitalId = new PdfDigitalId("GemBoxECDsa521.pfx", "GemBoxPassword");
            var signer = new PdfSigner(digitalId);

            // Adobe Acrobat Reader currently doesn't download the certificate chain
            // so we will also embed a certificate of intermediate Certificate Authority in the signature.
            // (see https://community.adobe.com/t5/acrobat/signature-validation-using-aia-extension-not-enabled-by-default/td-p/10729647)
            signer.ValidationInfo = new PdfSignatureValidationInfo(new PdfCertificate[] { new PdfCertificate("GemBoxECDsa.crt") }, null, null);

            signatureField.Sign(signer);

            document.Save("SignatureWithLockedFields.pdf");
        }
    }

    static void SignaturesWorkflow()
    {
        // If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = PdfDocument.Load("Reading.pdf"))
        {
            var textField = document.Form.Fields.AddText(document.Pages[0], 50, 530, 200, 20);
            textField.Name = "Field1";
            textField.Value = "Should be filled by the signer";

            // Signature field that is signed with the author permission.
            var authorSignatureField = document.Form.Fields.AddSignature();
            authorSignatureField.Name = "AuthorSignature";

            // Signature field that will be signed by another signer.
            var signatureField = document.Form.Fields.AddSignature(document.Pages[0], 300, 500, 250, 50);
            signatureField.Name = "Signature1";
            signatureField.SetLockedFields(textField);
            // After this signature field is signed, the document is final.
            signatureField.LockedFields.Permission = PdfUserAccessPermissions.None;

            var certifyingDigitalId = new PdfDigitalId("GemBoxRSA1024.pfx", "GemBoxPassword");
            var authorSigner = new PdfSigner(certifyingDigitalId);

            // Specify a certification signature with actions that are permitted after certifying the document.
            authorSigner.AuthorPermission = PdfUserAccessPermissions.FillForm;

            // Adobe Acrobat Reader currently doesn't download the certificate chain
            // so we will also embed a certificate of intermediate Certificate Authority in the signature.
            // (see https://community.adobe.com/t5/acrobat/signature-validation-using-aia-extension-not-enabled-by-default/td-p/10729647)
            authorSigner.ValidationInfo = new PdfSignatureValidationInfo(new PdfCertificate[] { new PdfCertificate("GemBoxRSA.crt") }, null, null);

            authorSignatureField.Sign(authorSigner);

            // Finish first signing of a PDF file.
            document.Save("CertificateAndApprovalSignaturesWorkflow.pdf");

            // Another signer fills its text field.
            textField.Value = "Filled by another signer";

            // And signs on its signature field thus making its text field locked.
            var approvalDigitalId = new PdfDigitalId("GemBoxECDsa521.pfx", "GemBoxPassword");
            var signer = new PdfSigner(approvalDigitalId);
            // Adobe Acrobat Reader currently doesn't download the certificate chain
            // so we will also embed a certificate of intermediate Certificate Authority in the signature.
            // (see https://community.adobe.com/t5/acrobat/signature-validation-using-aia-extension-not-enabled-by-default/td-p/10729647)
            signer.ValidationInfo = new PdfSignatureValidationInfo(new PdfCertificate[] { new PdfCertificate("GemBoxECDsa.crt") }, null, null);
            signatureField.Sign(signer);

            // Finish second signing of the same PDF file.
            document.Save();
        }
    }
}
