using System;
using System.Collections.Generic;
using System.IO;

namespace VKClient.Common.Framework
{
  public static class BinarySerializerExtensions
  {
    public static void WriteList<T>(this BinaryWriter writer, IList<T> list, int maxItemsToWrite = 10000) where T : IBinarySerializable
    {
      if (list != null)
      {
        int num1 = Math.Min(list.Count, maxItemsToWrite);
        writer.Write(num1);
        int num2 = 0;
        while (num2 < num1)
          list[num2++].Write(writer);
      }
      else
        writer.Write(0);
    }

    public static void WriteBoolNullable(this BinaryWriter writer, bool? b)
    {
      if (!b.HasValue)
        writer.Write(0);
      else if (b.Value)
        writer.Write(2);
      else
        writer.Write(1);
    }

    public static bool? ReadBoolNullable(this BinaryReader reader)
    {
      switch (reader.ReadInt32())
      {
        case 0:
          return new bool?();
        case 1:
          return new bool?(false);
        case 2:
          return new bool?(true);
        default:
          return new bool?();
      }
    }

    public static void WriteDictionary(this BinaryWriter writer, Dictionary<string, string> dictionary)
    {
      if (dictionary != null)
      {
        writer.Write(dictionary.Keys.Count);
        foreach (KeyValuePair<string, string> keyValuePair in dictionary)
        {
          writer.Write(keyValuePair.Key);
          writer.Write(keyValuePair.Value);
        }
      }
      else
        writer.Write(0);
    }

    public static void WriteDictionary(this BinaryWriter writer, Dictionary<long, long> dictionary)
    {
      if (dictionary != null)
      {
        writer.Write(dictionary.Keys.Count);
        foreach (KeyValuePair<long, long> keyValuePair in dictionary)
        {
          writer.Write(keyValuePair.Key);
          writer.Write(keyValuePair.Value);
        }
      }
      else
        writer.Write(0);
    }

    public static void WriteList(this BinaryWriter writer, List<string> list)
    {
      if (list != null)
      {
        writer.Write(list.Count);
        foreach (string str in list)
          writer.Write(str);
      }
      else
        writer.Write(0);
    }

    public static void WriteList(this BinaryWriter writer, List<long> list)
    {
      if (list != null)
      {
        writer.Write(list.Count);
        foreach (long num in list)
          writer.Write(num);
      }
      else
        writer.Write(0);
    }

    public static void WriteList(this BinaryWriter writer, List<int> list)
    {
      if (list != null)
      {
        writer.Write(list.Count);
        foreach (int num in list)
          writer.Write(num);
      }
      else
        writer.Write(0);
    }

    public static void Write<T>(this BinaryWriter writer, T value, bool trim = false) where T : IBinarySerializable
    {
      if (value != null)
      {
        writer.Write(true);
        if (trim && value is IBinarySerializableWithTrimSupport)
          (value as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
        else
          value.Write(writer);
      }
      else
        writer.Write(false);
    }

    public static void Write(this BinaryWriter writer, DateTime value)
    {
      writer.Write(value.Ticks);
    }

    public static void WriteString(this BinaryWriter writer, string value)
    {
      writer.Write(value ?? string.Empty);
    }

    public static T ReadGeneric<T>(this BinaryReader reader) where T : IBinarySerializable, new()
    {
      if (!reader.ReadBoolean())
        return default (T);
      T instance = Activator.CreateInstance<T>();
      instance.Read(reader);
      return instance;
    }

    public static List<string> ReadList(this BinaryReader reader)
    {
      int num = reader.ReadInt32();
      if (num <= 0)
        return new List<string>();
      List<string> stringList = new List<string>();
      for (int index = 0; index < num; ++index)
        stringList.Add(reader.ReadString());
      return stringList;
    }

    public static List<long> ReadListLong(this BinaryReader reader)
    {
      int num = reader.ReadInt32();
      if (num <= 0)
        return new List<long>();
      List<long> longList = new List<long>();
      for (int index = 0; index < num; ++index)
        longList.Add(reader.ReadInt64());
      return longList;
    }

    public static List<int> ReadListInt(this BinaryReader reader)
    {
      int num = reader.ReadInt32();
      if (num <= 0)
        return new List<int>();
      List<int> intList = new List<int>();
      for (int index = 0; index < num; ++index)
        intList.Add(reader.ReadInt32());
      return intList;
    }

    public static Dictionary<string, string> ReadDictionary(this BinaryReader reader)
    {
      int capacity = reader.ReadInt32();
      Dictionary<string, string> dictionary = new Dictionary<string, string>(capacity);
      for (int index = 0; index < capacity; ++index)
      {
        string key = reader.ReadString();
        string str = reader.ReadString();
        dictionary.Add(key, str);
      }
      return dictionary;
    }

    public static Dictionary<long, long> ReadDictionaryLong(this BinaryReader reader)
    {
      int capacity = reader.ReadInt32();
      Dictionary<long, long> dictionary = new Dictionary<long, long>(capacity);
      for (int index = 0; index < capacity; ++index)
      {
        long key = reader.ReadInt64();
        long num = reader.ReadInt64();
        dictionary.Add(key, num);
      }
      return dictionary;
    }

    public static List<T> ReadList<T>(this BinaryReader reader) where T : IBinarySerializable, new()
    {
      int num = reader.ReadInt32();
      if (num <= 0)
        return new List<T>();
      List<T> objList = new List<T>();
      for (int index = 0; index < num; ++index)
      {
        T instance = Activator.CreateInstance<T>();
        instance.Read(reader);
        objList.Add(instance);
      }
      return objList;
    }

    public static DateTime ReadDateTime(this BinaryReader reader)
    {
      return new DateTime(reader.ReadInt64());
    }
  }
}
