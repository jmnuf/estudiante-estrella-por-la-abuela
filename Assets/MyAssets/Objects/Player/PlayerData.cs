using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerLoadResult = Result<PlayerData, System.Exception>;

[System.Serializable]
public struct PlayerData {
	private static BindingFlags ATTRIBUTES_FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
	public static string folder_path { get => Application.persistentDataPath + "students"; }
	public string username { get; private set; }

	public Dictionary<string, object> to_dict() {
		var self = this;
		Dictionary<string, object> dict = get_properties().ToDictionary(
			prop_info => prop_info.Name,
			prop_info => prop_info.GetValue(self, null)
		);
		
		return dict;
	}

	private static string get_file_path(string username) {
		string file_name = username.Replace(" ", "_") + ".data";
		return Path.Combine(PlayerData.folder_path, file_name);
	}

	public bool save_file() {
		var data = to_dict();
		string file_path = PlayerData.get_file_path(username);
		Debug.Log(file_path);
		StreamWriter writer = new StreamWriter(file_path, false);
		foreach (string key in data.Keys) {
			object val = data[key];
			System.Type val_type = val.GetType();
			if (val is int) {
				writer.WriteLine(key + ": " + val.ToString());
			} else if (val is float) {
				writer.WriteLine(key + ": " + val.ToString() + "f");
			} else if (val is string) {
				writer.WriteLine(key + ": `" + ((string)val) + "`");
			}
		}
		writer.Close();
		return false;
	}

	public static PlayerLoadResult read_file(string username) {
		var dict = new SortedDictionary<string, object>();
		string file_path = get_file_path(username);
		StreamReader reader = new StreamReader(file_path);
#nullable enable
		string? line;
		for(line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
			int idx = line.IndexOf(":");
			if (idx < 1) {
				continue;
			}
			string[] kv = line.Split(':', System.StringSplitOptions.None);
			if (kv.Length < 2) {
				continue;
			}
			string prop_key = kv[0].Trim();
			string prop_raw = string.Join(':', kv.Skip(1)).Trim();
			object prop_val;
			try {
				if (prop_raw.StartsWith("`") && prop_raw.EndsWith("`")) {
					prop_raw = prop_raw.Where((_ch, idx) => idx != 0 && idx != prop_raw.Length - 1).ToString();
					prop_val = prop_raw;
				} else if (prop_raw.EndsWith("f")) {
					prop_raw = prop_raw.Where((ch, idx) => idx < prop_raw.Length - 1).ToString();
					prop_val = float.Parse(prop_raw);
				} else {
					prop_val = int.Parse(prop_raw);
				}
			} catch (Exception ex) {
				return PlayerLoadResult.Err(ex);
			}
			if (dict.ContainsKey(prop_key)) {
				dict.Remove(prop_key);
			}
			dict.Add(prop_key, prop_val);
		}
#nullable disable
		reader.Close();

		if (dict.Count == 0) {
			return PlayerLoadResult.Err(new Exception("No user data found within file"));
		}
		return from_dict(dict).match(
			(val) => PlayerLoadResult.Ok(val),
			() => PlayerLoadResult.Err(new Exception("No valid user data found"))
		);
	}

	private PropertyInfo[] get_properties() {
		return GetType().GetProperties(PlayerData.ATTRIBUTES_FLAGS);
	}

	public static Option<PlayerData> from_dict(SortedDictionary<string, object> dict) {
		var data = new PlayerData();
		var properties = data.get_properties();

		int set_values = 0;
		foreach (PropertyInfo prop_info in properties) {
			string name = prop_info.Name;
			if (!dict.ContainsKey(name)) {
				continue;
			}
			object val = dict[name];
			if (prop_info.PropertyType != val.GetType()) {
				continue;
			}
			prop_info.SetValue(data, val);
			set_values += 1;
		}
		if (set_values == 0) {
			return Option<PlayerData>.None;
		}
		return Option<PlayerData>.Some(data);
	}
}
