//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

////programmed with Liam, added by Lanielle

//public class FileDataHandler
//{
//    private string dataDirPath = "";

//    private bool useEncryption = false;
//    private readonly string encryptionCodeWord = "weArentReallyUsingThis";

//    public FileDataHandler(string dataDirPath, bool useEncryption)
//    {
//        this.dataDirPath = dataDirPath;
//        this.useEncryption = useEncryption;
//    }

//    public GameData Load(string profileID)
//    {
//        string fullPath = Path.Combine(dataDirPath, profileID);
//        GameData loadedData = null;
//        if (File.Exists(fullPath))
//        {
//            try
//            {
//                string dataToLoad = "";

//                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
//                {
//                    using (StreamReader reader = new StreamReader(stream))
//                    {
//                        dataToLoad = reader.ReadToEnd();
//                    }
//                }

//                if (useEncryption)
//                {
//                    dataToLoad = EncryptDecrypt(dataToLoad);
//                }

//                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
//            }
//            catch (Exception e)
//            {
//                Debug.LogError("Error occured when trying to load file: " + fullPath + "\n" + e);
//            }
//        }

//        return loadedData;
//    }

//    public void Save(GameData data, string profileID)
//    {
//        string fullPath = Path.Combine(dataDirPath, profileID);

//        try
//        {
//            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

//            string dataToStore = JsonUtility.ToJson(data, true);

//            if (useEncryption)
//            {
//                dataToStore = EncryptDecrypt(dataToStore);
//            }

//            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
//            {
//                using (StreamWriter writer = new StreamWriter(stream))
//                {
//                    writer.Write(dataToStore);
//                }
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogError("Error occured when trying to save file: " + fullPath + "\n" + e);
//        }
//    }

//    private string EncryptDecrypt(string data)
//    {
//        string modifiedData = "";
//        for (int i = 0; i < data.Length; i++)
//        {
//            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
//        }
//        return modifiedData;
//    }
//}

//public class GameData
//{
//    int relationshipScore;
//}