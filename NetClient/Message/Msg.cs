// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: msg.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace NetProtocol {

  /// <summary>Holder for reflection information generated from msg.proto</summary>
  public static partial class MsgReflection {

    #region Descriptor
    /// <summary>File descriptor for msg.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MsgReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgltc2cucHJvdG8SC05ldFByb3RvY29sGgptc2cyLnByb3RvGh9nb29nbGUv",
            "cHJvdG9idWYvVGltZXN0YW1wLnByb3RvItcBCgxTdG9yZVJlcXVlc3QSDAoE",
            "bmFtZRgBIAEoCRILCgNudW0YAiABKAUSDgoGcmVzdWx0GAMgASgFEg4KBm15",
            "TGlzdBgEIAMoCRIvCgNkaWMYBSADKAsyIi5OZXRQcm90b2NvbC5TdG9yZVJl",
            "cXVlc3QuRGljRW50cnkSLwoLbGFzdF91cGRhdGUYBiABKAsyGi5nb29nbGUu",
            "cHJvdG9idWYuVGltZXN0YW1wGioKCERpY0VudHJ5EgsKA2tleRgBIAEoCRIN",
            "CgV2YWx1ZRgCIAEoBToCOAEiQgoITWFwRmllbGQSCwoDa2V5GAEgASgFEikK",
            "BXZhbHVlGAIgASgLMhouTmV0UHJvdG9jb2wuU3RvcmVSZXF1ZXN0MmIGcHJv",
            "dG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::NetProtocol.Msg2Reflection.Descriptor, global::Google.Protobuf.WellKnownTypes.TimestampReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::NetProtocol.StoreRequest), global::NetProtocol.StoreRequest.Parser, new[]{ "Name", "Num", "Result", "MyList", "Dic", "LastUpdate" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, }),
            new pbr::GeneratedClrTypeInfo(typeof(global::NetProtocol.MapField), global::NetProtocol.MapField.Parser, new[]{ "Key", "Value" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class StoreRequest : pb::IMessage<StoreRequest> {
    private static readonly pb::MessageParser<StoreRequest> _parser = new pb::MessageParser<StoreRequest>(() => new StoreRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<StoreRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::NetProtocol.MsgReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public StoreRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public StoreRequest(StoreRequest other) : this() {
      name_ = other.name_;
      num_ = other.num_;
      result_ = other.result_;
      myList_ = other.myList_.Clone();
      dic_ = other.dic_.Clone();
      lastUpdate_ = other.lastUpdate_ != null ? other.lastUpdate_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public StoreRequest Clone() {
      return new StoreRequest(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "num" field.</summary>
    public const int NumFieldNumber = 2;
    private int num_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Num {
      get { return num_; }
      set {
        num_ = value;
      }
    }

    /// <summary>Field number for the "result" field.</summary>
    public const int ResultFieldNumber = 3;
    private int result_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Result {
      get { return result_; }
      set {
        result_ = value;
      }
    }

    /// <summary>Field number for the "myList" field.</summary>
    public const int MyListFieldNumber = 4;
    private static readonly pb::FieldCodec<string> _repeated_myList_codec
        = pb::FieldCodec.ForString(34);
    private readonly pbc::RepeatedField<string> myList_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> MyList {
      get { return myList_; }
    }

    /// <summary>Field number for the "dic" field.</summary>
    public const int DicFieldNumber = 5;
    private static readonly pbc::MapField<string, int>.Codec _map_dic_codec
        = new pbc::MapField<string, int>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForInt32(16, 0), 42);
    private readonly pbc::MapField<string, int> dic_ = new pbc::MapField<string, int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<string, int> Dic {
      get { return dic_; }
    }

    /// <summary>Field number for the "last_update" field.</summary>
    public const int LastUpdateFieldNumber = 6;
    private global::Google.Protobuf.WellKnownTypes.Timestamp lastUpdate_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Google.Protobuf.WellKnownTypes.Timestamp LastUpdate {
      get { return lastUpdate_; }
      set {
        lastUpdate_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as StoreRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(StoreRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Num != other.Num) return false;
      if (Result != other.Result) return false;
      if(!myList_.Equals(other.myList_)) return false;
      if (!Dic.Equals(other.Dic)) return false;
      if (!object.Equals(LastUpdate, other.LastUpdate)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Num != 0) hash ^= Num.GetHashCode();
      if (Result != 0) hash ^= Result.GetHashCode();
      hash ^= myList_.GetHashCode();
      hash ^= Dic.GetHashCode();
      if (lastUpdate_ != null) hash ^= LastUpdate.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Num != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Num);
      }
      if (Result != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Result);
      }
      myList_.WriteTo(output, _repeated_myList_codec);
      dic_.WriteTo(output, _map_dic_codec);
      if (lastUpdate_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(LastUpdate);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Num != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Num);
      }
      if (Result != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Result);
      }
      size += myList_.CalculateSize(_repeated_myList_codec);
      size += dic_.CalculateSize(_map_dic_codec);
      if (lastUpdate_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(LastUpdate);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(StoreRequest other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Num != 0) {
        Num = other.Num;
      }
      if (other.Result != 0) {
        Result = other.Result;
      }
      myList_.Add(other.myList_);
      dic_.Add(other.dic_);
      if (other.lastUpdate_ != null) {
        if (lastUpdate_ == null) {
          LastUpdate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        LastUpdate.MergeFrom(other.LastUpdate);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Num = input.ReadInt32();
            break;
          }
          case 24: {
            Result = input.ReadInt32();
            break;
          }
          case 34: {
            myList_.AddEntriesFrom(input, _repeated_myList_codec);
            break;
          }
          case 42: {
            dic_.AddEntriesFrom(input, _map_dic_codec);
            break;
          }
          case 50: {
            if (lastUpdate_ == null) {
              LastUpdate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(LastUpdate);
            break;
          }
        }
      }
    }

  }

  public sealed partial class MapField : pb::IMessage<MapField> {
    private static readonly pb::MessageParser<MapField> _parser = new pb::MessageParser<MapField>(() => new MapField());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MapField> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::NetProtocol.MsgReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapField() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapField(MapField other) : this() {
      key_ = other.key_;
      value_ = other.value_ != null ? other.value_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapField Clone() {
      return new MapField(this);
    }

    /// <summary>Field number for the "key" field.</summary>
    public const int KeyFieldNumber = 1;
    private int key_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Key {
      get { return key_; }
      set {
        key_ = value;
      }
    }

    /// <summary>Field number for the "value" field.</summary>
    public const int ValueFieldNumber = 2;
    private global::NetProtocol.StoreRequest2 value_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::NetProtocol.StoreRequest2 Value {
      get { return value_; }
      set {
        value_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MapField);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MapField other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Key != other.Key) return false;
      if (!object.Equals(Value, other.Value)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Key != 0) hash ^= Key.GetHashCode();
      if (value_ != null) hash ^= Value.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Key != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Key);
      }
      if (value_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Value);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Key != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Key);
      }
      if (value_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Value);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MapField other) {
      if (other == null) {
        return;
      }
      if (other.Key != 0) {
        Key = other.Key;
      }
      if (other.value_ != null) {
        if (value_ == null) {
          Value = new global::NetProtocol.StoreRequest2();
        }
        Value.MergeFrom(other.Value);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Key = input.ReadInt32();
            break;
          }
          case 18: {
            if (value_ == null) {
              Value = new global::NetProtocol.StoreRequest2();
            }
            input.ReadMessage(Value);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
