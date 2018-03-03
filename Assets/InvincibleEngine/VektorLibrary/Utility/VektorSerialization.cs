using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace InvincibleEngine.VektorLibrary.Utility {
    public static class Serialization {
        
        // Serialize a given object to a byte[]
        public static byte[] SerializeToBytes(object obj) {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            
            // Serialize the object
            formatter.Serialize(stream, obj);
            
            // return the byte[]
            return stream.ToArray();
        }
        
        // Compress a byte[]
        public static byte[] CompressBytesGzip(byte[] data) {
            var stream = new MemoryStream();
            var compressor = new GZipStream(stream, CompressionMode.Compress, true);
            
            // compress the data
            compressor.Write(data, 0, data.Length);
            compressor.Close();
            
            // return the compressed byte[]
            return stream.ToArray();
        }
        
        // Compress a byte[] (LZF)
        public static byte[] CompressBytesLzf(byte[] data) {
            var compressor = new LZF();
            var rawLzf = new byte[data.Length * 2];
            
            // compress the data
            var size = compressor.Compress(data, data.Length, rawLzf, rawLzf.Length);
            
            // grab the relevant data
            var compressed = new byte[size];
            for (var i = 0; i < size; i++) {
                compressed[i] = rawLzf[i];
            }
            
            // return the compressed byte[]
            return compressed;
        }
    }
}
