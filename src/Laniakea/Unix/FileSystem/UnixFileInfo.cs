using System.Runtime.InteropServices;
using Laniakea.Unix.CApi;
using Laniakea.Linux.CApi;

namespace Laniakea.Unix.FileSystem;

public class UnixFileInfo
{
    private FileInfo _fileInfo;

    public UnixFileInfo(string path)
    {
        _fileInfo = new FileInfo(path);
    }

    public string Path
    {
        get
        {
            return _fileInfo.FullName;
        }
    }

    public bool Exists => _fileInfo.Exists;

    public void SetOwnership(uint? uid, uint? gid)
    {
        uint user = uid ?? uint.MaxValue;
        uint group = gid ?? uint.MaxValue;

        int result = CUnistd.chown(Path, user, group);

        if (result == 0)
        {
            return;
        }
        else
        {
            int err = Marshal.GetLastSystemError();
            if (err == CErrno.EACCES)
            {
                throw new UnauthorizedAccessException();
            } else if (!Exists)
            {
                throw new FileNotFoundException();
            }
            else
            {
                throw new IOException();
            }
        }
    }
}
