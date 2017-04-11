using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Security.Principal;

using Affirma.ThreeSharp.Model;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Wrapper;


namespace Affirma.ThreeSharp.ConsoleSample
{
	class ThreeSharpConsoleSample
	{
		
		static void Main(string[] args)
		{
			try
			{
				//if (awsAccessKeyId.StartsWith("<INSERT") || awsSecretAccessKey.StartsWith("<INSERT"))
				//throw new Exception("You must edit the code and insert your access keys to run the samples.");
				//
				//TODO add a command line switch to allow new ids to be entered
				string LocalVersion="0.1.5";
				HTTPGet req = new HTTPGet();
				req.Request("http://12321.s3.amazonaws.com/wins3fs-version.txt");
				string WebVersion=req.ResponseBody;
				//if(String.Compare(LocalVersion,WebVersion)>0) {
				//	MessageBox.Show("Older Version Found", "WinS3fs Update", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				//}
				//if(String.Compare(LocalVersion,WebVersion)==0) {
				//	MessageBox.Show("Same Version Found", "WinS3fs Update", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				//}
				if(String.Compare(LocalVersion,WebVersion)<0) {
					if (DialogResult.Yes==MessageBoxEx.Show("Newer Version Found.\nDo you want to update?\nhttp://wins3fs.sf.net/", "WinS3fs Update", 
						MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2,3000)) {
					System.Diagnostics.Process p =
					new System.Diagnostics.Process();
					System.Diagnostics.ProcessStartInfo pi =
					new System.Diagnostics.ProcessStartInfo();
					pi.FileName = "http://wins3fs.sf.net/";
					p.StartInfo = pi;
					p.Start();
					return;
					}
				}

				int result;
				while((result=ThreeSharpConfig())!=0) {
					if (result<0) return; //config failed.  Most likely invaild awskeys
					//if result==1 then we re run config.
				}
				
				string service_name="WinS3FS";
				ServiceController[] scServices;
				scServices = ServiceController.GetServices();
				int found_winfuse_service=0;
				
				foreach (ServiceController scTemp in scServices)
				{

					if (scTemp.ServiceName == service_name)
					{
						found_winfuse_service=1;
						ServiceController sc = new ServiceController(service_name);
						sc.Stop();
						while (sc.Status != ServiceControllerStatus.Stopped)
						{
							Thread.Sleep(1000);
							sc.Refresh();
						}
						sc.Start();
						while (sc.Status == ServiceControllerStatus.Stopped)
						{
							Thread.Sleep(1000);
							sc.Refresh();
						}
						/*				if (sc.Status == ServiceControllerStatus.Stopped)
					{
						sc.Start();
						while (sc.Status == ServiceControllerStatus.Stopped)
						{
							Thread.Sleep(1000);
							sc.Refresh();
						}
					}
	*/
					}
				}
				
				if (found_winfuse_service==0) {
					//stop the running winfuse process
					Process[] myProcesses;
					//Returns array containing all instances of Notepad.
					myProcesses = Process.GetProcessesByName("wins3fs");
					foreach (Process myProcess in myProcesses)
					{
						myProcess.Kill();
					}
					Process.Start("wins3fs.exe","-CONFIG");
				}
				/*Process p = new Process();
				p.StartInfo.WorkingDirectory = "c:\\";
				p.StartInfo.Arguments = @"stop WinFUSE";
				p.StartInfo.FileName = @"net.exe";
				p.Start();
				p.StartInfo.Arguments = @"start WinFUSE";
				p.StartInfo.FileName = @"net.exe";
				p.Start();
				*/
			}
			catch (ThreeSharpException ex)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("An exception occurred.");
				sb.AppendLine("Message: " + ex.Message);
				sb.AppendLine("HTTP Status Code: " + ex.StatusCode.ToString());
				sb.AppendLine("Error Code: " + ex.ErrorCode);
				sb.AppendLine("XML: " + ex.XML);
				Console.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		static int ThreeSharpConfig()
		{
			// Convert the bucket name to lowercase for vanity domains.
			// the bucket must be lower case since DNS is case-insensitive.
			// returns -1 if the aws keys are not vaild after several attempts to query the user
			// returns 0 if configuration was successful
			// returns 1 if configureation needs to be run again after adding a bucket.
			
			String OutputString="";
			String OutputTemplate="";

			StreamReader sr = File.OpenText("WinS3FS.exe.conf-template"); 
			String s="";
			Regex rx = new Regex("awsAccessKeyId");
			while ((s = sr.ReadLine()) != null) 
			{
				if(rx.IsMatch(s)){
					int Template_once=0;
					//need to extract awsAccessKeyId, awsSecretAccessKey
					//string aws_ids[]=s.Split(' ');
					//Regex.Match(s,"awsAccessKeyId");
					//Regex awsrx = new Regex("awsAccessKeyId");
					
					string awsAccessKeyId = "";
					string awsSecretAccessKey = "";

					int aws_index;

					if((aws_index=s.IndexOf("\"",s.IndexOf("awsAccessKeyId")))>0) {
						if (s.Length>aws_index+21) {
							awsAccessKeyId=s.Substring(aws_index+1,20); //need to check that there is text available
						}
					};
					
					
					if((aws_index=s.IndexOf("\"",s.IndexOf("awsSecretAccessKey")))>0) {
						string description; //should be "AWSKEY" after decrypt
						//if (s.Length>aws_index+41) {
							//awsSecretAccessKey=s.Substring(aws_index+1,40); //need to check that there is text available
						if (s.Length>aws_index+265) {
							awsSecretAccessKey= DPAPI.Decrypt(s.Substring(aws_index+1,264),
                                WindowsIdentity.GetCurrent().User.ToString(), out description);
								//awsSecretAccessKey=s.Substring(aws_index+1,226); //need to check that there is text available
						}
					};
					
					//see if we can access the list of buckets, thereby cheching the awsAccess keys provided
					int list_bucket_success=0;
					bool add_bucket=false;
					ThreeSharpWrapper wrapper = new ThreeSharpWrapper(awsAccessKeyId, awsSecretAccessKey); 
					String allBuckets="";
					while (list_bucket_success<2) {  //let the use try to enter valid it two times
						try {
							allBuckets=wrapper.ListBucket(null);
							list_bucket_success=10;
						}
						catch {
							//MessageBox.Show("Provide vaild awsAccessKeyId numbers", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							awsAccessKeyId = Interaction.InputBox("Please enter a valid awsAccessKeyId","Bucket","awsAccessKeyId",100,100);
							awsSecretAccessKey = Interaction.InputBox("Please enter a valid awsSecretAccessKey","Bucket","awsSecretAccessKey",100,100);
							
							wrapper = new ThreeSharpWrapper(awsAccessKeyId, awsSecretAccessKey); 
							list_bucket_success++;
						}
					}
					
					if (list_bucket_success!=10) {
						MessageBoxEx.Show("Aws config failed\nWins3fs not started", "WinS3fs ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,3000);
						return -1;
					}
					
					//rebuild the template so we don't have to ask for the aws ids every time.
					if (Template_once==0) {
					    string encawskey=DPAPI.Encrypt( DPAPI.KeyType.UserKey, awsSecretAccessKey, WindowsIdentity.GetCurrent().User.ToString(), "AWSKEY");
						OutputTemplate+="\t\t<add name=\"username\" awsAccessKeyId=\""+awsAccessKeyId+"\" awsSecretAccessKey=\""+encawskey+"\" />\n";
						Template_once=1;
					}
					String Name="";
					
					
					//prase the returned bucket list from ListBucket() and make a list of available buckets
					XmlDocument objDocument = new XmlDocument();
					objDocument.LoadXml(allBuckets);

					//File.AppendAllText("winfuse-config.log",allBuckets);
					list_bucket_success=0;
					XmlNodeList elemList = objDocument.GetElementsByTagName("DisplayName");

					for (int i=0; i < elemList.Count; i++)
					{   
						// Display all book titles in the Node List.
						//Console.WriteLine(elemList[i].InnerXml);
						Name=elemList[i].InnerXml;
					}
					//Console.WriteLine();

					try {
						elemList = objDocument.GetElementsByTagName("Name");
					}
					catch { //if we have an error parsing the xml list then we are missing a bucket
						add_bucket=true;
					}

					if (elemList.Count==0 || add_bucket==true) {
						//have user enter a default bucket if none is present
						list_bucket_success=0;
						Random r = new Random();
						do {
							++list_bucket_success;
							string BucketName = Interaction.InputBox("Please enter a unique bucketname:","Bucket","default"+r.Next(50000).ToString(),100,100);
							try {
								add_bucket=true;
								wrapper.AddBucket(BucketName);
							}
							catch { //bucket already present
								add_bucket=false;
							}
							
							if (list_bucket_success==2) {
								MessageBoxEx.Show("Create bucket failed\nWins3fs not started", "WinS3fs ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,3000);
								return -1;
							}
							
							if (add_bucket==true) {
							    string encawskey=DPAPI.Encrypt( DPAPI.KeyType.UserKey, awsSecretAccessKey, WindowsIdentity.GetCurrent().User.ToString(), "AWSKEY");
								OutputString+="\t\t<add name=\""+BucketName+"\" description=\"S3 "+BucketName+"\" type=\"Suchwerk.FileSystem.S3FS, S3FS\" mappedpath=\""+
								BucketName+"\" awsAccessKeyId=\""+awsAccessKeyId+"\" awsSecretAccessKey=\""+encawskey+"\" />\n";
							}
						}while (add_bucket==false);
						//return 1; //rerun config 
					}
					
					for (int i=0; i < elemList.Count; i++)
					{   
						//walk the xml list of buckets and create a list
						string encawskey=DPAPI.Encrypt( DPAPI.KeyType.UserKey, awsSecretAccessKey, WindowsIdentity.GetCurrent().User.ToString(), "AWSKEY");
						OutputString+="\t\t<add name=\""+elemList[i].InnerXml+"\" description=\"S3 "+elemList[i].InnerXml+"\" type=\"Suchwerk.FileSystem.S3FS, S3FS\" mappedpath=\""+
						elemList[i].InnerXml+"\" awsAccessKeyId=\""+awsAccessKeyId+"\" awsSecretAccessKey=\""+encawskey+"\" />\n";
						//Console.WriteLine(elemList[i].InnerXml);
					}
					//OutputString+="<change me>\n";
				} else {
					OutputString+=s+"\n";
					OutputTemplate+=s+"\n";
				}
			}
			sr.Close();
			
			OutputString+="\n";
			//OutputTemplate+="\n";
			
			//OutputString+=File.ReadAllText("WinFUSE.exe.config2");
			File.WriteAllText("WinS3FS.exe.config",OutputString);
			File.WriteAllText("WinS3FS.exe.conf-template",OutputTemplate);
			//WinFuse.Close();
			return 0; //config complete
		}
	}
}

