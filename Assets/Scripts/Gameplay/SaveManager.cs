using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class SaveManager : BaseComponent
    {
        SavedDatas datas = null;

        [SerializeField]
        string saveFileName = "Save.json";

        string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

        protected override bool IsValid() => !string.IsNullOrEmpty(saveFileName);

        #region Save
        public void Save(SavedDatas newDatas = null)
        {
            SavedDatas datasToSave = newDatas ?? datas;
            if(datasToSave == null)
            {
                Debug.LogWarning("No data to save.");
                return;
            }

            string jsonDatas = JsonUtility.ToJson(datasToSave);
            string encryptedJSON = Encrypt(jsonDatas);
            File.WriteAllText(SavePath, encryptedJSON);
        }

        string Encrypt(string jsonDatas)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonDatas);
            byte[] key = Encoding.UTF8.GetBytes(EnvVariables.AES_KEY);
            byte[] iv = Encoding.UTF8.GetBytes(EnvVariables.AES_IV);

            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(jsonBytes, 0, jsonBytes.Length);
                            cs.FlushFinalBlock(); 

                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
        }

        public bool ContainsSave()
        {
            return datas != null || File.Exists(SavePath);
        }
        #endregion
        

        #region Load
        public SavedDatas Load()
        {
            if(File.Exists(SavePath))
            {
                string encryptedJSON = File.ReadAllText(SavePath);
                string jsonDatas = Decrypt(encryptedJSON);
                datas = JsonUtility.FromJson<SavedDatas>(jsonDatas);
            }
            
            return datas;
        }

        string Decrypt(string encryptedJSON)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedJSON);
            byte[] key = Encoding.UTF8.GetBytes(EnvVariables.AES_KEY);
            byte[] iv = Encoding.UTF8.GetBytes(EnvVariables.AES_IV);

            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                            cs.FlushFinalBlock();

                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                }
            }
        }
        #endregion
    }
}