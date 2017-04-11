//#define SERVICE

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Configuration.Install;
using NeoGeo.Library.SMB;
using NeoGeo.Library.SMB.Provider;

#if(!SERVICE)
using System.Drawing;
using System.Windows.Forms;

#endif

namespace Palissimo.WinFUSE
{
#if (SERVICE)
	public class SuchwerkService : ServiceBase
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		//private IConfigAdapter Config;			// Muss global sein damit Config-Klasse nie entladen wird !!
		private MyTextWriterTraceListener LogFileListner;
		
		public SuchwerkService()
		{
			// Dieser Aufruf ist für den Windows Komponenten-Designer erforderlich.
			InitializeComponent();
			
			// TODO: Initialisierungen nach dem Aufruf von InitComponent hinzufügen
		}

		// Der Haupteinstiegspunkt für den Vorgang
		[MTAThread]
		static void Main(string[] args)
		{
			if (args.Length==0)
			{	// Service Case, so start normal
				ServiceBase ToRun;
				ToRun = new SuchwerkService();

				ServiceBase.Run(ToRun);
				return;
			}

			if (args.Length != 0)
			{
				SelfServiceInstaller si = new SelfServiceInstaller();
				switch (args[0].ToUpper())
				{
				case "I":
				case "-I":
				case "-INSTALL":
					si.InstallMe();
					break;
				case "U":
				case "-U":
				case "-UNINSTALL":
					si.UnInstallMe();
					break;
				default:
					
					//System.Windows.Forms.MessageBox.Show("Nur die Schalter -I für Install und -U für Un-Install werden unterstützt.");
					break;
				}
			}
		}

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "Suchwerk";
			this.AutoLog = true;
			this.CanHandlePowerEvent = false;
			this.CanPauseAndContinue = false;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Führen Sie die Vorgänge aus, um den Dienst zu starten.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			string DataPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			
			if (System.IO.File.Exists(DataPath))
			{
				System.IO.File.Delete(DataPath);
				LogFileListner = new MyTextWriterTraceListener(DataPath);
				Trace.Listeners.Add(LogFileListner);
			}
			Trace.AutoFlush = true;
			Debug.AutoFlush = true;

			Thread InitThread = new Thread(new ThreadStart(StartServer));
			InitThread.Start();
		}

		/// <summary>
		/// Beenden Sie den Dienst.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Hier Code zum Ausführen erforderlicher Löschvorgänge zum Anhalten des Dienstes einfügen.
			StopServer();
		}

#else
		public class TestForm : System.Windows.Forms.Form
		{
			private System.Windows.Forms.Button button1;
			private System.Windows.Forms.Button button2;
			private System.Windows.Forms.NotifyIcon notifyIcon1;
			private System.Windows.Forms.ContextMenu contextMenu1;
			private System.Windows.Forms.MenuItem menuExit;
			private System.Windows.Forms.MenuItem menuHide;
			private System.Windows.Forms.MenuItem menuShow;
			private System.Windows.Forms.MenuItem menuAddBucket;
			internal System.Windows.Forms.TextBox TraceTextBox;
			private System.ComponentModel.Container components = null;
			public System.Windows.Forms.CheckBox CBFatal;
			public System.Windows.Forms.CheckBox CBWarnung;
			public System.Windows.Forms.CheckBox CBInfo;
			public Label Status;
			
			private MyTextWriterTraceListener LogFileListner;
			private delegate void SetTextCallback(string Text);

			public TestForm()
			{
				InitializeComponent();

				//Wird für VS2005 benötigt, dort wird die Thread-Safeheit überprüft
				TextBox.CheckForIllegalCrossThreadCalls = false;

				string DataPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
				if (!DataPath.EndsWith("\\"))
				DataPath += "\\";
				
				System.IO.File.Delete(DataPath + "log.txt");
				LogFileListner = new MyTextWriterTraceListener(DataPath + "log.txt", this);
				Trace.Listeners.Add(LogFileListner);
				
				Trace.AutoFlush = true;
				Debug.AutoFlush = true;

				//this.Show();
				this.Hide();
				this.WindowState = FormWindowState.Minimized;
				Application.DoEvents();

				StartServer();
			}

			public void SetText(string Text)
			{
				// Sollte den Eintrag, ThreadSave machen, funktioniert aber nicht, deshalb auskommentiert
				//	if (this.TraceTextBox.InvokeRequired)
				//	{
				//		SetTextCallback call = new SetTextCallback(SetText);
				//		this.Invoke(call, new object[] { Text });
				//	}
				//	else
				{
					this.TraceTextBox.Text += Text + Environment.NewLine;
					this.TraceTextBox.Select(TraceTextBox.Text.Length, 0);
					this.TraceTextBox.ScrollToCaret();
				}
				Application.DoEvents();
			}


			/// <summary>
			/// Die verwendeten Ressourcen bereinigen.
			/// </summary>
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose(disposing);
			}

			#region Vom Windows Form-Designer generierter Code
			/// <summary>
			/// Erforderliche Methode für die Designerunterstützung. 
			/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
			/// </summary>
			private void InitializeComponent()
			{
				this.components = new System.ComponentModel.Container();
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));

				this.button1 = new System.Windows.Forms.Button();
				this.button2 = new System.Windows.Forms.Button();
				this.CBFatal = new System.Windows.Forms.CheckBox();
				this.TraceTextBox = new System.Windows.Forms.TextBox();
				this.CBWarnung = new System.Windows.Forms.CheckBox();
				this.CBInfo = new System.Windows.Forms.CheckBox();
				this.Status = new System.Windows.Forms.Label();
				this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(); //this.components
				this.contextMenu1 = new System.Windows.Forms.ContextMenu();
				this.menuExit = new System.Windows.Forms.MenuItem();
				this.menuShow = new System.Windows.Forms.MenuItem();
				this.menuHide = new System.Windows.Forms.MenuItem();
				this.menuAddBucket = new System.Windows.Forms.MenuItem();
				this.SuspendLayout();
				// 
				// button1 Exit
				// 
				this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.button1.Location = new System.Drawing.Point(744, 304);
				this.button1.Name = "button1";
				this.button1.Size = new System.Drawing.Size(88, 24);
				this.button1.TabIndex = 4;
				this.button1.Text = "&Exit";
				this.button1.Click += new System.EventHandler(this.button1_Click);
				// 
				// button2 Hide
				// 
				this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.button2.Location = new System.Drawing.Point(644, 304);
				this.button2.Name = "button2";
				this.button2.Size = new System.Drawing.Size(88, 24);
				this.button2.TabIndex = 5;
				this.button2.Text = "&Hide";
				this.button2.Click += new System.EventHandler(this.button2_Click);
				// 
	            // notifyIcon1
	            // 
				int menu_index=0;
				
				this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.menuAddBucket});
				this.menuAddBucket.Index = menu_index++;
		        this.menuAddBucket.Text = "Add Bucket";
		        this.menuAddBucket.Click += new System.EventHandler(this.menuAddBucket_Click);
				
				this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.menuShow});
				this.menuShow.Index = menu_index++;
		        this.menuShow.Text = "S&how";
		        this.menuShow.Click += new System.EventHandler(this.menuShow_Click);

				this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.menuHide});
				this.menuHide.Index = menu_index++;
		        this.menuHide.Text = "H&ide";
		        this.menuHide.Click += new System.EventHandler(this.menuHide_Click);

				this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.menuExit});
				this.menuExit.Index = menu_index++;
		        this.menuExit.Text = "E&xit";
		        this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
				
	            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
	            this.notifyIcon1.BalloonTipTitle = "WinS3FS";
	            this.notifyIcon1.ContextMenu = this.contextMenu1;
	            //this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
				this.notifyIcon1.Icon = new System.Drawing.Icon("Simple.ico");
	            this.notifyIcon1.Text = "WinS3FS";
	            this.notifyIcon1.Visible = true;
	            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
				// 
	            // contextMenuStrip1
	            // 
	            //this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
	            //this.exitToolStripMenuItem});
	            //this.contextMenuStrip1.Name = "contextMenuStrip1";
	            //this.contextMenuStrip1.Size = new System.Drawing.Size(104, 26);
				// 
	            // exitToolStripMenuItem
	            // 
	            //this.exitToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
	            //this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
	            //this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
	            //this.exitToolStripMenuItem.Text = "&Exit";
	            //this.exitToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
	            //this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
				// 
				// CBFatal
				// 
				this.CBFatal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.CBFatal.Checked = true;
				this.CBFatal.CheckState = System.Windows.Forms.CheckState.Checked;
				this.CBFatal.Location = new System.Drawing.Point(16, 304);
				this.CBFatal.Name = "CBFatal";
				this.CBFatal.Size = new System.Drawing.Size(72, 16);
				this.CBFatal.TabIndex = 1;
				this.CBFatal.Text = "Fatal";
				// 
				// TraceTextBox
				// 
				this.TraceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
				this.TraceTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				this.TraceTextBox.Location = new System.Drawing.Point(8, 8);
				this.TraceTextBox.Multiline = true;
				this.TraceTextBox.Name = "TraceTextBox";
				this.TraceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
				this.TraceTextBox.Size = new System.Drawing.Size(824, 288);
				this.TraceTextBox.TabIndex = 0;
				// 
				// CBWarnung
				// 
				this.CBWarnung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.CBWarnung.Checked = true;
				this.CBWarnung.CheckState = System.Windows.Forms.CheckState.Checked;
				this.CBWarnung.Location = new System.Drawing.Point(96, 304);
				this.CBWarnung.Name = "CBWarnung";
				this.CBWarnung.Size = new System.Drawing.Size(88, 16);
				this.CBWarnung.TabIndex = 2;
				this.CBWarnung.Text = "Warning";
				// 
				// CBInfo
				// 
				this.CBInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.CBInfo.Location = new System.Drawing.Point(200, 304);
				this.CBInfo.Name = "CBInfo";
				this.CBInfo.Size = new System.Drawing.Size(96, 16);
				this.CBInfo.TabIndex = 3;
				this.CBInfo.Text = "Info";
				// 
				// Status
				// 
				this.Status.AutoSize = true;
				this.Status.BackColor = System.Drawing.Color.Red;
				this.Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
				this.Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				this.Status.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
				this.Status.Location = new System.Drawing.Point(261, 304);
				this.Status.Name = "Status";
				this.Status.Size = new System.Drawing.Size(45, 15);
				this.Status.TabIndex = 5;
				this.Status.Text = "Status";
				this.Status.Visible = false;
				// 
				// Form1
				// 
				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.ClientSize = new System.Drawing.Size(840, 334);
				this.ControlBox = false;
				this.MaximizeBox = false;
				this.ShowInTaskbar = false;
				//this.ControlBox.Click += new System.EventHandler(this.button_close);
				this.Controls.Add(this.Status);
				this.Controls.Add(this.CBInfo);
				this.Controls.Add(this.CBWarnung);
				this.Controls.Add(this.TraceTextBox);
				this.Controls.Add(this.CBFatal);
				this.Controls.Add(this.button1);
				this.Controls.Add(this.button2);
				
				//this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
				this.Icon = new System.Drawing.Icon("Simple.ico");
				this.Name = "WinFUSE";
				this.Text = "WinFUSE";
				this.ResumeLayout(false);
				this.PerformLayout();

			}
			#endregion

			[STAThread]
			static void Main(string[] args)
			{
				if (args.Length != 0)
				{
					switch (args[0].ToUpper())
					{
					case "-C":
					case "-CONFIG":
						//we have been told not to run the config  program
						Application.Run(new TestForm());
						return;
					default:
						Process.Start("wins3fs-config.exe");
						//System.Windows.Forms.MessageBox.Show("Nur die Schalter -I für Install und -U für Un-Install werden unterstützt.");
						return;
					}
				}
				//Application.Run(new TestForm());
				Process.Start("wins3fs-config.exe");

			}

			private void button1_Click(object sender, System.EventArgs e)
			{
				this.notifyIcon1.Visible = false;
				StopServer();
				this.Close();
			}
			
			private void button2_Click(object sender, System.EventArgs e)
			{
	                this.Hide();
					this.WindowState = FormWindowState.Minimized;
			}

		    private void menuExit_Click(object Sender, EventArgs e) {
				this.notifyIcon1.Visible = false;
				StopServer();
		        this.Close();
		    }
			
		    private void menuShow_Click(object Sender, EventArgs e) {
	                this.Show();
	                this.WindowState = FormWindowState.Normal;
		    }

		    private void menuHide_Click(object Sender, EventArgs e) {
					this.Hide();
					this.WindowState = FormWindowState.Minimized;
		    }			
		    
			private void menuAddBucket_Click(object Sender, EventArgs e) {
					//MessageBox.Show("Bucket","Adding bucket");
		    }
			
			private void notifyIcon1_DoubleClick(object sender, EventArgs e)
	        {
	            if (this.WindowState == FormWindowState.Minimized)
	            {
	                this.Show();
	                this.WindowState = FormWindowState.Normal;
	            } else {
	                this.Hide();
					this.WindowState = FormWindowState.Minimized;
				}

	            // Activate the form.
	            this.Activate();
	            this.Focus();
	        }

			//private void exitToolStripMenuItem_Click(object sender, EventArgs e)
	        //{
	            //If we don't minimize the window first we won't be able to close the window.
	        //    this.WindowState = FormWindowState.Minimized;
			//	StopServer();
	        //    this.Close();
	        //}
#endif

			SMB CIFS = null;

			//Eigentliche Funktionalität des Programms !
			public void StartServer()
			{
				System.Threading.Thread.CurrentThread.Name = "Main-Thread";
				Trace.WriteLine("-------------- Start of Trace --------------");

				// Test of Suchwerk schon läuft
				if (RunAlready())
				{
					Trace.WriteLine("Fatal->Program is already running, this instance will be stopped.");
					throw new ApplicationException("Program must not started more than once.");
				}

				const int SMBNameLength = 15;
				// SMB Server starten
				try
				{
					//Servernamen von Suchwerk erstellen
					string HostName = System.Windows.Forms.SystemInformation.ComputerName.ToUpper();

					if (HostName.Length > (SMBNameLength))
					HostName = HostName.Substring(0, SMBNameLength);

					//string ServerName = "%X-SW";		// Vorschrift zum Bauen des ServerNamens, %X wird durch Rechnernamen ersetzt
					string ServerName = ConfigurationManager.AppSettings["ServerName"];
					string DomainController = ConfigurationManager.AppSettings["DomainController"];
					//string awsAccessKeyId = ConfigurationManager.AppSettings["awsAccessKeyId"];
					//MessageBox.Show(awsAccessKeyId, "Config Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				
				
					if (ServerName.IndexOf("%X") == -1)
					{
						if (ServerName.Length > SMBNameLength)
						ServerName = ServerName.Substring(0, SMBNameLength);
					}
					else
					{	// We have someting to replace
						int copy = SMBNameLength - (ServerName.Length - 2); // 14 ist maximallänge - anhang - length("%x")
						if (HostName.Length <= copy)
						ServerName = ServerName.Replace("%X", HostName);
						else
						ServerName = ServerName.Replace("%X", HostName.Substring(0, copy));
					}

					CIFS = new SMB();

					CIFS.Start(
					ServerName,
					(ushort)16384,
					NetFlag.Announce | NetFlag.RemoteAccess | NetFlag.ListenAllNetworkCards,
					5, // Anzahl der gleichzeitigen Anfragen
					DomainController);
				}
				catch (Exception ex)
				{
					throw new ApplicationException("CIFS server could not be startet, error: " + ex.Message);
				}
			}

			public void StopServer()
			{
				CIFS.Stop(false, 1000);

				Trace.WriteLine("-------------- End of Trace --------------");
				Trace.Flush();
				Debug.Flush();
			}

			private static bool RunAlready()
			{
				bool CreateNew;
				string MutexGuid = "{91FD0A7F-0E9A-4FE2-A4DC-5E20316F9BEF}";
				System.Threading.Mutex mtx = new System.Threading.Mutex(false, MutexGuid, out CreateNew);
				return (!CreateNew);
			}	// RunAlready


		}  // class


		// Helper-Funktionen für Logging
#if (!SERVICE)
		public class MyTextWriterTraceListener : TraceListener
		{
			private TextWriterTraceListener Output;
			private TestForm Form;

			public MyTextWriterTraceListener(string Path, TestForm form)
			{
				Output = new TextWriterTraceListener(Path);
				Form = form;
			}

			public override void Write(string Text)
			{
				Output.Write(Text);
				//Form.TraceTextBox.Text += Text;
				//Form.TraceTextBox.ScrollToCaret();
			}

			public override void WriteLine(string Text)
			{
				if (Text.StartsWith("Info->") && !Form.CBInfo.Checked)
				return;

				if ((Text.StartsWith("Warnung->") || Text.StartsWith("Warning->")) && !Form.CBWarnung.Checked)
				return;

				if (Text.StartsWith("Fatal->") && !Form.CBFatal.Checked)
				return;

				if (Text.StartsWith("Online->"))
				{
					Form.Status.Text = Text.Substring("Online->".Length);
					Form.Status.Visible = true;
					Application.DoEvents();
					return;
				}
				if (Text.StartsWith("Offline->"))
				{
					Form.Status.Visible = false;
					Application.DoEvents();
					return;
				}
				
				StackFrame SF = new StackFrame(3, false);
				string s = SF.GetMethod().Name.PadRight(20);
				s = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + s + " : " + Text;
				Output.WriteLine(s);
				Form.SetText(s);
				//Form.TraceTextBox.Text += s + Environment.NewLine;
				//Form.TraceTextBox.Select(Form.TraceTextBox.Text.Length, 0);
				//Form.TraceTextBox.ScrollToCaret();
			}
		}
#else
		public class MyTextWriterTraceListener : TraceListener
		{
			// Ins Eventlog schreiben aufnehmen !!!!
			private TextWriterTraceListener Output;
			
			public MyTextWriterTraceListener(string Path)
			{	
				Output = new TextWriterTraceListener(Path);
			}
			
			public override void Write(string Text)
			{
				Output.Write(Text);
			}

			public override void WriteLine(string Text)
			{
				StackFrame SF = new StackFrame(3, false);
				string s = SF.GetMethod().Name.PadRight(20);
				s = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + s + " : " + Text;
				Output.WriteLine(s);
			}
		}

#endif

	}
	[RunInstallerAttribute(true)]
	public class MyProjectInstaller : Installer
	{
		private ServiceInstaller serviceInstaller;
		private ServiceProcessInstaller processInstaller;

		public MyProjectInstaller()
		{
			// Instantiate installers for process and services.
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			// The services run under the system account.
			processInstaller.Account = ServiceAccount.LocalSystem;

			// The services are started manually.
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			// ServiceName must equal those on ServiceBase derived classes.            
			serviceInstaller.ServiceName = "WinS3FS";

			// Add installers to collection. Order is not important.
			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}
	}
