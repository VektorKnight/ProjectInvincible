using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using SteamNet;

namespace HexSerializer {
   
    /// <summary>
    /// No subclasses please
    /// </summary>
    public static class HexSerialize {

        //String defining the namespace of serialized messages
        private const string NamespaceKey = "SteamNet";

        /// <summary>
        /// Accepts a list of classes, serializes into an array of bytes with proper headers
        /// </summary>
        /// 
        ///-----each segment-----
        ///{1 byte segment size} [0] size of entire segment excluding this byte
        ///{1 byte header size}
        ///{0-255 byte UTF-8 type header}
        ///{1 byte syncfield size}
        ///TODO:{1 byte syncfield index}
        ///{variable size syncfield raw data from type- int float v3 etc...}
        ///----------------------


        //packs collection of items into data, stops when reaching maxBuffer and returns false if exceding max buffer
        public static bool Zip(this List<byte> source, object input, int maxBuffer) {

            //byte representation of this segment
            List<byte> bytes = new List<byte>();

            //for each object, start by fetching all fields
            FieldInfo[] fields = GetReflectionFields(input.GetType());
            List<object> fieldValues = new List<object>();
            for (int i = 0; i < fields.Length; i++) {
                fieldValues.Add(fields[i].GetValue(input));
            }

            //populate a list with the byte representations of their values tagged with their byte size
            foreach (var f in fieldValues) {
                ///if this object is a collection we pack it in a different way
                ///instead of the variable size syncfield raw data, that portion is replaced with
                ///[1 byte size][1 byte first element size][first element as zipped message][...]
                byte[] b;

                if (IsGenericEnumerable(f.GetType())) {
                    List<byte> p = new List<byte>();
                    foreach (var n in (IEnumerable)f) {
                        List<byte> x = new List<byte>();
                        if (!x.Zip(n, maxBuffer)) {
                            return false;
                        }
                        p.InsertRange(0, x);
                        p.Insert(0, (byte)x.Count);
                    }
                    b = p.ToArray();
                }
                else {
                    byte[] p;
                    p = ToByte(f);
                    b = p;
                }
                byte size = (byte)b.Length;
                bytes.Add(size);
                bytes.AddRange(b);
            }

            ///at this moment the segment reads [1 byte size header][syncfield raw data] repeated for 
            ///all variables

            //now tag the segment with all headers and add to the collection
            byte[] typeHeader = ToByte(input.GetType().Name);
            bytes.InsertRange(0, typeHeader);
            bytes.Insert(0, (byte)typeHeader.Length);

            //size header
            ushort m = (ushort)bytes.Count;
            bytes.InsertRange(0, ToByte(m));

            //segment is processed and ready, check if the destination list can store it
            if (bytes.Count > (maxBuffer - source.Count)) {
                Debug.Log("MAX BUFFER HIT");
                return false;
            }

            //if the bytes we want to put in is less than our remaining amount, go ahead and add
            else {
                source.AddRange(bytes);
                return true;
            }
        }

        //unzips data serialized in this form, returns list of all objects in data stream
        public static List<AmbiguousTypeHolder> Unzip(IEnumerable<byte> data) {
            //create list from input to split
            List<byte> dataToUnpack = data.ToList();

            //break apart into data parts
            List<List<byte>> segments = new List<List<byte>>();

            //list to return
            List<AmbiguousTypeHolder> returns = new List<AmbiguousTypeHolder>();

            //keep going through the list until all data has been unpacked
            while (dataToUnpack.Count > 0) {
                segments.Add(dataToUnpack.SubList<byte>(2));
            }

            //we have all segments decoded, iterate through each 
            foreach (List<byte> n in segments) {

                //grab object name header from segment
                string header = (string)FromByte(n.SubList<byte>(1).ToArray(), typeof(string));
                Type headerType = Type.GetType($"{NamespaceKey}.{header}");
                
                //create object from magic
                var obj = FormatterServices.GetUninitializedObject(headerType);

                //determine what fields will be grabbed from rest of segment
                FieldInfo[] fieldInfo = GetReflectionFields(headerType);

                //iterate through remainder of segment getting all data from reflection fields
                List<object> setFields = new List<object>();
                for (int i = 0; i < fieldInfo.Length; i++) {

                    //determine type
                    Type type = fieldInfo[i].FieldType;

                    //get raw data from header
                    List<byte> c = n.SubList<byte>(1);
                    //add field
                    //if dealing with collection usw different meathod
                    if (IsGenericEnumerable(type)) {
                        Type IType = type.GetGenericArguments()[0];
                        var Result = Activator.CreateInstance(typeof(List<>).MakeGenericType(IType));
                        List<byte> x = c;
                        while (x.Count > 0) {
                            object objTemp = Unzip(x.SubList<byte>(1))[0].obj;
                            Result.GetType().GetMethod("Add").Invoke(Result, new[] { objTemp });
                        }

                        //Resultant list is in reverse order, reverse it to get it correct
                        Result.GetType().GetMethod("Reverse", new Type[] { }).Invoke(Result, new object[] { });
                        setFields.Add(Result);

                    }
                    else {
                        setFields.Add(FromByte(c.ToArray(), type));
                    }
                }

                //set all values of fields to data in segment
                SetReflectionFields(obj, setFields.ToArray());

                //add new object to list
                returns.Add(new AmbiguousTypeHolder(obj, headerType));
            }

            return returns;
        }

        public static byte[] ToByte(object src) {

            //if this object is one of our own, serialize in classic way
            if (src.GetType().Namespace.Contains("SteamNet")) {
                List<byte> n = new List<byte>();
                n.Zip(src, 1400);
                return n.ToArray();
            }
            if (src is float) {
                return BitConverter.GetBytes((Single)src);
            }
            if (src is int) {
                return BitConverter.GetBytes((int)src);
            }
            if (src is Int32) {
                return BitConverter.GetBytes((Int32)src);
            }
            if (src is string) {
                return Encoding.ASCII.GetBytes((string)src);
            }
            if (src is ushort) {
                return BitConverter.GetBytes((ushort)src);
            }
            if (src is ulong) {
                return BitConverter.GetBytes((ulong)src);
            }
            if (src is short) {
                return BitConverter.GetBytes((Int16)src);
            }
            if (src is bool) {
                return BitConverter.GetBytes((bool)src);
            }
            if (src is Vector3) {
                byte[] bytes = new byte[12]; // 4 bytes per float
                Vector3 vsrc = (Vector3)src;

                byte[] buff = new byte[sizeof(float) * 3];
                Buffer.BlockCopy(BitConverter.GetBytes(vsrc.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(vsrc.y), 0, buff, 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(vsrc.z), 0, buff, 2 * sizeof(float), sizeof(float));

                return buff;
            }
            if (src is Vector2) {
                byte[] bytes = new byte[8]; // 4 bytes per float
                Vector2 vsrc = (Vector2)src;

                byte[] buff = new byte[sizeof(float) * 3];
                Buffer.BlockCopy(BitConverter.GetBytes(vsrc.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(vsrc.y), 0, buff, 1 * sizeof(float), sizeof(float));

                return buff;
            }
            if (src is byte) {
                return new byte[(byte)src];
            }

            Debug.LogError($"Writer could not resolve type {src.GetType()}");
            return null;
        }

        public static object FromByte(byte[] src, Type type) {
            //if object is ours, deserialize in classic way

            if (type.Namespace == "SteamNet") {
                return Unzip(src.ToList())[0].obj;
            }
            if (type == typeof(float)) {
                return BitConverter.ToSingle(src, 0);

            }
            if (type == typeof(int)) {
                return BitConverter.ToInt32(src, 0);
            }
            if (type == typeof(Int32)) {
                return BitConverter.ToInt32(src, 0);
            }
            if (type == typeof(string)) {
                return Encoding.ASCII.GetString(src);
            }
            if (type == typeof(bool)) {
                return BitConverter.ToBoolean(src, 0);
            }
            if (type == typeof(short)) {
                return BitConverter.ToInt16(src, 0);
            }
            if (type == typeof(ushort)) {
                return BitConverter.ToUInt16(src, 0);
            }
            if (type == typeof(ulong)) {
                return BitConverter.ToUInt64(src, 0);
            }
            if (type == typeof(Vector3)) {
                Vector3 vect = Vector3.zero;
                vect.x = BitConverter.ToSingle(src, 0 * sizeof(float));
                vect.y = BitConverter.ToSingle(src, 1 * sizeof(float));
                vect.z = BitConverter.ToSingle(src, 2 * sizeof(float));
                return vect;
            }
            if (type == typeof(Vector2)) {
                Vector2 vect = Vector2.zero;
                vect.x = BitConverter.ToSingle(src, 0 * sizeof(float));
                vect.y = BitConverter.ToSingle(src, 1 * sizeof(float));
                return vect;
            }
            if (type == typeof(byte)) {
                return src[0];
            }
            Debug.LogError($"Writer could not resolve type {type}");
            return null;
        }


        public static T[] SubArray<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);

            return result;

        }

        static bool IsGenericEnumerable(Type t) {
            var genArgs = t.GetGenericArguments();
            if (genArgs.Length == 1 &&
                    typeof(IEnumerable<>).MakeGenericType(genArgs).IsAssignableFrom(t))
                return true;
            else
                return t.BaseType != null && IsGenericEnumerable(t.BaseType);
        }

        /// <summary>
        /// Snips the length of data off a list defined by header and returns the snipped section
        /// </summary>
        /// <param name="data">List to cut</param>
        /// <param name="sizeOfHeader"></param>
        /// <returns></returns>
        public static List<byte> SubList<Byte>(this List<byte> data, int sizeOfHeader) {
            //Debug.Log($"Getting the first {sizeOfHeader} bytes from array size of {data.Count}");
            byte[] header = data.GetRange(0, sizeOfHeader).ToArray();
            int sizeOfBuffer = 0;
            if (header.Length == 1) {
                sizeOfBuffer = header[0];
            }
            if (header.Length == 2) {
                sizeOfBuffer = (ushort)FromByte(header, typeof(ushort));
            }

            List<byte> returnList = data.GetRange(0 + header.Length, sizeOfBuffer);
            data.RemoveRange(0, sizeOfBuffer + header.Length);
            return returnList;
        }

        /// <summary>
        /// returns sorted list of public fields to access their values/types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetReflectionFields(Type type) {
            //summon fields
            List<FieldInfo> allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList<FieldInfo>();

            //determine if any have custom attributes
            foreach (FieldInfo prop in allFields) {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs) {
                    SyncField authAttr = attr as SyncField;
                    if (authAttr == null) {
                        
                        allFields.Remove(prop);
                    }
                }
            }

            //Debug.Log($"Found {syncFields.Count} fields");
            return allFields.OrderBy(o => o.Name).ToArray();
        }

        /// <summary>
        /// Sets public binding flagged fields by a sorted array of objects, types are not necessary
        /// </summary>
        /// <param name="set"></param>
        /// <param name="target"></param>
        public static void SetReflectionFields(this object target, object[] set) {

            FieldInfo[] f = GetReflectionFields(target.GetType());
            for (int i = 0; i < f.Length; i++) {
                if (i > set.Length)
                    return;
                f[i].SetValue(target, set[i]);
            }

        }

        // Gameobject Manipulation Methods
        public static List<Component> GetComponentsSorted(GameObject input) {
            List<Component> components = new List<Component>();
            foreach (Component n in input.GetComponents<Component>()) {

                //we only care about those components that are ours and sort them
                if (n.GetType().ToString().Contains("InvincibleEngine")) {
                    components.Add(n);
                    components.OrderBy(o => o.GetType().ToString());
                }
            }
            return components;

        }

        /// <summary>
        /// Get all fields from all subclasses in the gameobject
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<SyncField> GetObjectSyncFields(this GameObject input) {

            //search through all components for those that are our own scripts
            List<SyncField> Fields = new List<SyncField>();
            
            try {
               foreach(Component n in GetComponentsSorted(input)) {

                }
                
            }
            finally {

            }
            return null;
        }

        /// <summary>
        /// Set all feilds from all subclasses in the gameobject
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool SetObjectSyncFields(this GameObject input, List<object> fields) {
            return true;
        }
    }
}
