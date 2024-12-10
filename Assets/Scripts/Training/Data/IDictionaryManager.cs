using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Training.Data
{
    public interface IDictionaryManager
    {
        //void InitializeFirebase();
        void ValidateWordFirebase(string word, Action<bool> callback);
        IEnumerator ValidateWord(string word, System.Action<bool> resultCallback);
        void SaveWordInDictionary(string word, Action<bool> callback);

        int CheckWords(string currentLetters, string adyacentLetter);

        void MigrateWordsFromEnToEs();
    }
}
