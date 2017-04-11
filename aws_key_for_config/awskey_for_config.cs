
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
/// <summary>
/// Demonstrates the use of DPAPI functions to encrypt and decrypt data.
/// </summary>
public class DPAPITest
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
			Stream inputStream = Console.OpenStandardInput();
	        byte[] bytes = new byte[100];
	        Console.WriteLine("Enter AWSSecretKey: ");
			
	        int outputLength = inputStream.Read(bytes, 0, 100);
            Console.WriteLine("\nPaste the following into WinFUSE.exe.conf-template to add additional accounts:\n");
			//string entropy = WindowsIdentity.GetCurrent().User.ToString();
            //string description;

            //Console.WriteLine(text);

            // Call DPAPI to encrypt data with user-specific key.
            string encrypted = DPAPI.Encrypt( DPAPI.KeyType.UserKey,
                                              bytes.ToString(),
                                              WindowsIdentity.GetCurrent().User.ToString(),
                                              "AWSKEY");
            Console.WriteLine(encrypted+"\n");

            // Call DPAPI to decrypt data.
            //string decrypted = DPAPI.Decrypt(   encrypted,
            //                                    WindowsIdentity.GetCurrent().User.ToString(),
            //                                out description);
            //Console.WriteLine("{0} <<<{1}>>>\r\n", decrypted, description);
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                Console.WriteLine(ex.Message);
                ex = ex.InnerException;
            }
        }
    }
}