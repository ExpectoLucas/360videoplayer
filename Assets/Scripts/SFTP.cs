/*
 * Thanks to https://ourcodeworld.com/articles/read/369/how-to-access-a-sftp-server-using-ssh-net-sync-and-async-with-c-in-winforms
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;           // Exceptions
using System.IO;
using Renci.SshNet;     // SFTP library

public class SFTP
{
    private string host = null; //@"gemini.science.uu.nl";
    private string username = null; //"6493769";
    private string password = null; //@"6Dzeta&7Eta";
    //private string remoteDirectory = "/science-nfs-sys/vsm01/users/6493769/www/UUCSVR";
    private string remoteDirectory = "/";

    /// <summary>
    /// Construct SFTP object
    /// </summary>
    /// <param name="hostIP"></param>
    /// <param name="userName"></param>
    /// <param name="passWord"></param>
    public SFTP(string hostIP, string userName, string passWord) { host = hostIP; username = userName; password = passWord; }

    /// <summary>
    /// List a remote directory in the console.
    /// </summary>
    public void ListFiles()
    {
        using (SftpClient sftp = new SftpClient(host, username, password))
        {
            try
            {
                sftp.Connect();

                var files = sftp.ListDirectory(remoteDirectory);

                foreach (var file in files)
                {
                    Debug.Log(file.Name);
                }

                sftp.Disconnect();
            }
            catch (Exception e)
            {
                Debug.Log("An exception has been caught " + e.ToString());
            }
        }
    }

    /// <summary>
    /// Downloads a file in the desktop synchronously
    /// </summary>
    public void DownloadFile(string fileName)
    {
        // Path to file on SFTP server
        string pathRemoteFile = remoteDirectory+"/"+fileName;
        // Path where the file should be saved once downloaded (locally)
        string pathLocalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "download_sftp_file.txt");

        using (SftpClient sftp = new SftpClient(host, username, password))
        {
            try
            {
                sftp.Connect();

                Console.WriteLine("Downloading {0}", pathRemoteFile);

                using (Stream fileStream = File.OpenWrite(pathLocalFile))
                {
                    sftp.DownloadFile(pathRemoteFile, fileStream);
                }

                sftp.Disconnect();
            }
            catch (Exception er)
            {
                Console.WriteLine("An exception has been caught " + er.ToString());
            }
        }
    }

    /// <summary>
    /// Delete a remote file
    /// </summary>
    private void DeleteFile(string fileName)
    {
        // Path to folder on SFTP server
        string pathRemoteFileToDelete = remoteDirectory+"/"+fileName;

        using (SftpClient sftp = new SftpClient(host, username, password))
        {
            try
            {
                sftp.Connect();

                // Delete file
                sftp.DeleteFile(pathRemoteFileToDelete);

                sftp.Disconnect();
            }
            catch (Exception er)
            {
                Console.WriteLine("An exception has been caught " + er.ToString());
            }
        }
    }
}
