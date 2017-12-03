using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    class SaveFile
    {
        // SaveFile manager
        /*
         * Save file in Documents/Collect2D/scores.dat
         * 
         */
        string dirName;
        string fileName;
        string filePath;

        public SaveFile()
        {
            dirName = "My Games";
            fileName = "TimeAttack.sav";

            // Create game savedata directory if it doesn't exist
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + dirName))
            {
                try
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + dirName);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error creating games directory: \n" + e.Message);
                }
            }

            this.filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + this.dirName + "\\" + this.fileName;
        }

        public SaveFile(string dirName, string fileName) : this()
        {
            this.dirName = dirName;
            this.fileName = fileName;
        }

        public string[] Open()
        {
            try
            {
                string[] data = File.ReadAllLines(filePath);

                return data;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error opening savefile: \n" + e.Message);
                return null;
            }
        }

        public bool Write(string[] data)
        {
            try
            {
                File.WriteAllLines(filePath, data);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error saving game data file: \n" + e.Message);
                return false;
            }
        }
    }
}
