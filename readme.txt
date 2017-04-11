WinS3FS by joelhewitt

An Amazon S3 WINFUSE filesystem for Windows

To use:


1.  Firstly create a S3 account at amazonaws.com.
In the course of registering, copy your awsAccessKeyId and awsSecretAccessKey.


2.  Run wins3fs.exe, this will make the WinS3FS.exe.conf file
populating it with your buckets.

If this is the first time you ran winfuse then you will be asked for your
awsAccessKeyId and awsSecretAccessKey keys.  Additionally if you haven't used
Amazon S3 before, you will be asked to create a bucket. Otherwise WinS3FS will
use your existing buckets.

If the help program wins3fs-config.exe is run after winfuse.exe has alread been started, 
wins3fs.exe will be restart. 

It would not be advisable to run winfuse-config.exe if you are in the middle 
of copying a file from S3 via WinS3FS.

4.  Assuming you were able to run winfuse and enter you aws keys, and
perhaps enter a bucket name, you should be presented with "WinS3FS" icon in
the tray bar. Right clickeing on this icon will preset a menu of options.
"Exit" is self explanitory, "Add Bucket" allow you to add extra buckets.
Clicking "show" and/or "hide" will reveal a window with a text output area.
During the alpha/beta release of this software this will show debugging 
information. If WinS3FS is compiled as a service this window will not show.
Note in the future running WinS3FS as a service will no be supported.

5.  open a command prompt and run "net view \\s3" or open a windows explorer
window and type in \\s3, and you should see a list of buckets you have accessable.

6.  if in a command prompt type dir \\s3\<bucket name>  to see the files in that bucket
or in windows explorer open the bcuket you would like to use.

7.  Copy your files.

8.  All files copied to your S3 bucket are make pubically readable. So if you
had copied a file name "index.htm" into S3, you can now from your web browser
enter "<bucketname>.s3.amazonaws.com/index.htm" and view the file you made.



If the above instructions do not get WinS3FS working, make sure you have .Net
intalled on your computer.  Make sure you did not delete a quotation mark in
the WinFUSE.exe.conf-template.  Do you have a bucket available in S3?

BUGS:

As of Sept 22, 2008:
--You can copy from S3 to your local computer, and get the file properties.
And you can copy files into S3.  At the moment copying seems only to work
from the command prompt, and only when you do a dir on the bucket:

(start winfuse after it has been properly configured)
dir \\s3\bucketname
copy \\s3\bucketname\filename c:\localpath\localname 
or
copy c:\localpath\localname \\s3\bucketname\filename 


The reason for this is that the filesystem is constantly looking that directories
and files. When they are local the time to look at a file is small. But when we need
to send out a request to S3 and parse the XML every time we inspect a file, things get
bogged down. So when you perform a directory listing the contents are saved for
future use.  If you try to copy a file and have not performed a dir, the directory
contents are blank or invalid.

--You cannot change the ACL of a file. You cannot create a subdirectory in your bucket.

--Theoretically you could copy files from one bucket to another, but I have not tried it.

--Subdirectories in S3 are not handeled properly, WinS3FS tries to download the
file named "directory/filename" 

--If the bucket name exceeds Windows maximum for SMB shares (15 characters), the
bucket name is truncated in the \\s3 directory listing, but the full name needs
to be provided to cd into it.  But everything works ok after that.

--As the ThreeSharp library does not allow byte ranges of a file to be
downloaded, the entire file is downloaded at once to a temporary file on the
local computer.  Then that temp file is passed on to the filesystem half which
copies the file to the destination intended.  This wreaks havok with the
"progress bar" since the entire file is retreived from S3 with no intervention
from the computer, and once the file is locally present, the progress bar
progresses, albeit quickly. The the temp file is deleted

Enjoy.

Joel

2008 by Joel Hewitt

THIS SOFTWARE IS PROVIDED "AS IS" AND WITHOUT ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
WARRANTIES OF MERCHANTIBILITY AND FITNESS FOR A PARTICULAR PURPOSE.
