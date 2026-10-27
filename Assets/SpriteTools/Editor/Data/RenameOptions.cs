using System;

namespace SpriteTools
{
    public enum NameCase
    {
        Unchanged,
        Lower,
        Upper
    }

    /// <summary>
    /// everything the rename tab lets me set packed into one object
    /// the RenamePanel fills it in and hands it to SpriteRenameService, which is the only thing that actually reads it
    /// </summary>
    [Serializable]
    public class RenameOptions
    {
        public string find = "";
        public string replace = "";
        public string prefix = "";
        public string suffix = "";
        public bool replaceSpaces = true;
        public NameCase nameCase = NameCase.Unchanged;
        public bool numbering;
        public string baseName = "";
        public int startNumber = 1;
        public int padding = 2;
        public string separator = "_";
    }
}
