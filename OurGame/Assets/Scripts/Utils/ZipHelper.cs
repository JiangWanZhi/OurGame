//解压Zip包
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class ZipHelper
{
    static byte[] buffer = new byte[2048];
    public static string ZipString(string text)
    {
        var o = ZipByte(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(o);
    }

    public static string UnzipString(string zipText)
    {
        var o = UnzipByte(Convert.FromBase64String(zipText));
        return Encoding.UTF8.GetString(o);
    }

    public static byte[] ZipByte(byte[] data)
    {
        using (var to = new MemoryStream())
        using (var zip = new GZipOutputStream(to))
        {
            zip.Write(data, 0, data.Length);
            zip.Finish();
            return to.ToArray();
        }
    }

    public static byte[] UnzipByte(byte[] data)
    {
        using (var from = new MemoryStream(data))
        using (var zip = new GZipInputStream(from))
        using (var to = new MemoryStream())
        {
            var len = 0;
            while ((len = zip.Read(buffer, 0, buffer.Length)) > 0)
            {
                to.Write(buffer, 0, len);
            }
            return to.ToArray();
        }
    }
    private const string DES_KEY_STR = "aabbccdd";
    private const string DES_IV_STR = "eeffgghh";
    public static byte[] EncryptData(byte[] src, bool compress = false)
    {
        var _buff = compress ? ZipByte(src) : src;
        using (DESCryptoServiceProvider _desp = new DESCryptoServiceProvider())
        {
            _desp.Key = Encoding.UTF8.GetBytes(DES_KEY_STR);
            _desp.IV = Encoding.UTF8.GetBytes(DES_IV_STR);
            _desp.Mode = CipherMode.CBC;
            _desp.Padding = PaddingMode.PKCS7;
            using (ICryptoTransform _xfrm = _desp.CreateEncryptor())
            {
                return _xfrm.TransformFinalBlock(_buff, 0, _buff.Length);
            }
        }
    }

    public static byte[] DecryptData(byte[] src, bool decompress = false)
    {
        using (DESCryptoServiceProvider _desp = new DESCryptoServiceProvider())
        {
            _desp.Key = Encoding.UTF8.GetBytes(DES_KEY_STR);
            _desp.IV = Encoding.UTF8.GetBytes(DES_IV_STR);
            _desp.Mode = CipherMode.CBC;
            _desp.Padding = PaddingMode.PKCS7;
            using (ICryptoTransform _xfrm = _desp.CreateDecryptor())
            {
                var _buff = _xfrm.TransformFinalBlock(src, 0, src.Length);
                return decompress ? UnzipByte(_buff) : _buff;
            }
        }
    }

    public static bool IsDataEncrypt(byte[] src)
    {
        try
        {
            DecryptData(src);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool UnzipFile(string pZipFilePath,       //zip文件路径
                                 string pOutPath,           //解压路径
                                 string pPassword = null,   //密码
                                 UnzipCallback pCB = null)  //回调
    {
        if (string.IsNullOrEmpty(pZipFilePath) || string.IsNullOrEmpty(pOutPath))
        {
            pCB?.OnFinished(false);
            return false;
        }

        return UnzipFile(File.OpenRead(pZipFilePath), pOutPath, pPassword, pCB);
    }

    //解压Zip包
    public static bool UnzipFile(byte[] pFileBytes,         //zip文件二进制数据
                                 string pOutPath,           //解压目录
                                 string pPassword = null,   //密码
                                 UnzipCallback pCB = null)  //回调
    {
        if (null == pFileBytes || string.IsNullOrEmpty(pOutPath))
        {
            pCB?.OnFinished(false);
            return false;
        }

        return UnzipFile(new MemoryStream(pFileBytes), pOutPath, pPassword, pCB);
    }

    //解压Zip
    public static bool UnzipFile(Stream pInputStream,       //Zip文件流
                                 string pOutPath,           //解压路径
                                 string pPassword = null,   //密码
                                 UnzipCallback pCB = null)  //回调
    {
        if (null == pInputStream || string.IsNullOrEmpty(pOutPath))
        {
            pCB?.OnFinished(false);
            return false;
        }

        // 创建文件目录
        if (!Directory.Exists(pOutPath))
            Directory.CreateDirectory(pOutPath);

        // 解压Zip包
        using (var zipInputStream = new ZipInputStream(pInputStream))
        {
            if (!string.IsNullOrEmpty(pPassword))
                zipInputStream.Password = pPassword;

            ZipEntry entry = null;
            while (null != (entry = zipInputStream.GetNextEntry()))
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                if (null != pCB && !pCB.OnPreUnzip(entry))
                    continue; // 过滤

                var filePathName = Path.Combine(pOutPath, entry.Name);

                // 创建文件目录
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(filePathName);
                    continue;
                }

                // 写入文件
                try
                {
                    using (var fileStream = File.Create(filePathName))
                    {
                        var bytes = new byte[2048];
                        while (true)
                        {
                            var count = zipInputStream.Read(bytes, 0, bytes.Length);
                            if (count > 0)
                            {
                                fileStream.Write(bytes, 0, count);
                            }
                            else
                            {
                                pCB?.OnPostUnzip(entry);
                                break;
                            }
                        }
                    }
                }
                catch (Exception _e)
                {
                    Debug.LogError("[UnzipFile]: " + _e);
                    GC.Collect();
                    pCB?.OnFinished(false);
                    return false;
                }
            }
        }

        GC.Collect();
        pCB?.OnFinished(true);
        return true;
    }

    //压缩文件和文件夹
    public static bool Zip(string[] pFileOrDirArray,    //需要压缩的文件和文件夹
                           string pZipFilePath,         //输出的zip文件完整路径
                           string pPassword = null,     //密码
                           ZipCallback pCB = null,      //回调
                           int pZipLevel = 6)           //压缩等级
    {
        if (null == pFileOrDirArray || string.IsNullOrEmpty(pZipFilePath))
        {
            pCB?.OnFinished(false);
            return false;
        }

        var zipOutputStream = new ZipOutputStream(File.Create(pZipFilePath));
        zipOutputStream.SetLevel(pZipLevel); // 6 压缩质量和压缩速度的平衡点
        zipOutputStream.Password = pPassword;

        foreach (string fileOrDirectory in pFileOrDirArray)
        {
            var result = false;

            if (Directory.Exists(fileOrDirectory))
                result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, pCB);
            else if (File.Exists(fileOrDirectory))
                result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, pCB);

            if (!result)
            {
                GC.Collect();
                pCB?.OnFinished(false);
                return false;
            }
        }

        zipOutputStream.Finish();
        zipOutputStream.Close();
        zipOutputStream = null;

        GC.Collect();
        pCB?.OnFinished(true);
        return true;
    }

    //压缩文件
    private static bool ZipFile(string pFileName,                   //需要压缩的文件名
                                string pParentPath,                 //相对路径
                                ZipOutputStream pZipOutputStream,   //压缩输出流
                                ZipCallback pCB = null)             //回调
    {
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string path = pParentPath + Path.GetFileName(pFileName);
            entry = new ZipEntry(path) { DateTime = DateTime.Now };

            if (null != pCB && !pCB.OnPreZip(entry))
                return true; // 过滤

            fileStream = File.OpenRead(pFileName);
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            entry.Size = buffer.Length;

            pZipOutputStream.PutNextEntry(entry);
            pZipOutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipFile]: " + _e);
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        pCB?.OnPostZip(entry);

        return true;
    }

    //压缩文件夹
    private static bool ZipDirectory(string pDirPath,                   //文件夹路径
                                     string pParentPath,                //相对路径
                                     ZipOutputStream pZipOutputStream,  //压缩输出流
                                     ZipCallback pCB = null)            //回调
    {
        ZipEntry entry = null;
        string path = Path.Combine(pParentPath, GetDirName(pDirPath));
        try
        {
            entry = new ZipEntry(path)
            {
                DateTime = DateTime.Now,
                Size = 0
            };

            if (null != pCB && !pCB.OnPreZip(entry))
                return true; // 过滤

            pZipOutputStream.PutNextEntry(entry);
            pZipOutputStream.Flush();

            var files = Directory.GetFiles(pDirPath);
            foreach (string file in files)
                ZipFile(file, Path.Combine(pParentPath, GetDirName(pDirPath)), pZipOutputStream, pCB);
        }
        catch (Exception _e)
        {
            Debug.LogError("[ZipDirectory]: " + _e);
            return false;
        }

        var directories = Directory.GetDirectories(pDirPath);
        foreach (string dir in directories)
            if (!ZipDirectory(dir, Path.Combine(pParentPath, GetDirName(pDirPath)), pZipOutputStream, pCB))
                return false;

        pCB?.OnPostZip(entry);

        return true;
    }

    private static string GetDirName(string pPath)
    {
        if (!Directory.Exists(pPath))
            return string.Empty;

        pPath = pPath.Replace("\\", "/");
        var _Ss = pPath.Split('/');
        if (string.IsNullOrEmpty(_Ss[_Ss.Length - 1]))
            return _Ss[_Ss.Length - 2] + "/";
        return _Ss[_Ss.Length - 1] + "/";
    }

    //压缩回调接口
    public interface ZipCallback
    {
        bool OnPreZip(ZipEntry _entry); //true表示继续执行
        void OnPostZip(ZipEntry _entry);
        void OnFinished(bool _result);
    }

    //解压缩接口
    public interface UnzipCallback
    {
        bool OnPreUnzip(ZipEntry _entry); //true表示继续执行
        void OnPostUnzip(ZipEntry _entry);
        void OnFinished(bool _result);
    }
}