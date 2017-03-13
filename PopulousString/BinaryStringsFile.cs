using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Contains all of the functionality for working with Populous strings.
/// </summary>
namespace PopulousString
{
    /// <summary>
    /// Provides functions for reading and writing the binary strings file used by Populous: The Beginning.
    /// The file is formatted such that each string is encoded as a Unicode, UTF-16 sequence of characters
    /// terminated by a NULL character (which is also two bytes).  The null-terminators also make it possible
    /// delimit two strings.
    /// </summary>
    public static class BinaryStringsFile
    {
        #region Public Methods
        /// <summary>
        /// Reads the strings from the specified file.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the file to read.</param>
        /// <returns>The resulting collection of strings read from the file.</returns>
        /// <exception cref="Exception">Thrown if reading the file fails.</exception>
        public static List<string> Read(string filePath)
        {
            // Much of the logic that follows can throw exceptions for various reasons, so we'll catch it and throw a more generic
            // exception to the caller, should a failure occur.
            try
            {
                // READ THE CONTENTS OF THE STRINGS FILE INTO MEMORY.
                // For the sake of simplicity, the whole file is buffered into memory.  This should be
                // acceptable since these files are typically very small (a few hundred Kilobytes).
                byte[] fileBytes = null;
                using (BinaryReader fileReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    // VERIFY THAT THE FILE IS A VALID LENGTH.
                    // Since we can only read up to int max number of bytes, the file length cannot
                    // exceed that amount.  This should not impose an issues since these files are
                    // typically very small.
                    long streamLengthInBytes = fileReader.BaseStream.Length;
                    if (streamLengthInBytes > int.MaxValue)
                    {
                        throw new Exception("The file is too large and cannot be read.");
                    }

                    // Verify that the length of the file is an even number of bytes.  This is important
                    // since the strings are encoded using Unicode (UTF-16), so each character (including
                    // NULLs) is represented by two bytes.
                    bool fileLengthIsEven = ((streamLengthInBytes % 2) == 0);
                    if (!fileLengthIsEven)
                    {
                        throw new Exception("The file contains invalid data and cannot be read.");
                    }

                    // READ THE BYTES INTO MEMORY.
                    int fileLengthInBytes = (int)fileReader.BaseStream.Length;
                    fileBytes = fileReader.ReadBytes(fileLengthInBytes);

                    // CLOSE THE FILE.
                    fileReader.Close();
                }

                // EXTRACT THE STRINGS FROM THE BUFFERED BYTES.
                // Since each character is represented by two byte's, we're going to read two bytes at a time and step forward
                // in increments of two bytes.
                int currentStringStartIndex = 0;
                const int BytesPerCharacter = 2;
                List<string> strings = new List<string>();
                for (int currentByteIndex = 0; currentByteIndex < fileBytes.Length; currentByteIndex += BytesPerCharacter)
                {
                    // CHECK TO SEE IF THE CURRENT BYTE PAIR IS NULL.
                    const byte NullByte = 0;
                    byte firstByte = fileBytes[currentByteIndex];
                    byte secondByte = fileBytes[currentByteIndex + 1];
                    bool nullBytePairFound = ((firstByte == 0) && (secondByte == NullByte));
                    if (nullBytePairFound)
                    {
                        // EXTRACT THE CURRENT STRING FROM THE FILE BYTES.
                        int currentStringEndIndex = currentByteIndex;
                        string extractedString = ExtractUnicodeStringFromBytes(fileBytes, currentStringStartIndex, currentStringEndIndex);

                        // ADD THE EXTRACTED STRING TO THE LIST OF STRINGS.
                        strings.Add(extractedString);

                        // START A NEW STRING BY UPDATING THE BYTE INDEX FOR THE NEXT STRING.
                        // The bytes / character are added to advance beyond this character for the
                        // start of the next string.
                        currentStringStartIndex = (currentByteIndex + BytesPerCharacter);
                    }
                }

                // CHECK TO SEE IF AN UNTERMINATED STRING EXISTS AT THE END OF THE STRINGS FILE.
                bool unterminatedStringExists = (currentStringStartIndex < fileBytes.Length);
                if (unterminatedStringExists)
                {
                    // EXTRACT THE UNTERMINATED STRING.
                    string extractedString = ExtractUnicodeStringFromBytes(fileBytes, currentStringStartIndex, fileBytes.Length);

                    // ADD THE EXTRACTED STRING TO THE LIST OF STRINGS.
                    strings.Add(extractedString);
                }

                return strings;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to read strings from file.", exception);
            }
        }

        /// <summary>
        /// Writes the collection of strings to the specified strings file.  Any existing
        /// strings file will be replaced by the new one.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the file to write.</param>
        /// <param name="strings">The strings to write to the file.</param>
        /// 
        public static void Write(string filePath, IEnumerable<string> strings)
        {
            // Much of the logic that follows can throw exceptions for various reasons, so we'll catch it and throw a more generic
            // exception to the caller, should a failure occur.
            try
            {
                // CREATE THE OUTPUT FILE.
                using (BinaryWriter fileWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    // WRITE EACH OF THE STRINGS TO THE OUTPUT FILE.
                    foreach (string currentString in strings)
                    {
                        // WRITE THE UNICODE STRING, AS BYTES, TO THE OUTPUT FILE.
                        byte[] stringAsBytes = Encoding.Unicode.GetBytes(currentString);
                        fileWriter.Write(stringAsBytes);

                        // APPEND A NULL-TERMINATION CHARACTER.
                        // The null-terminator separates the current string from the next one
                        // that will be written.
                        byte[] nullTerminationCharacterBytes = new byte[] { 0, 0 };
                        fileWriter.Write(nullTerminationCharacterBytes);
                    }

                    // CLOSE THE FILE.
                    fileWriter.Close();
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to write strings file.", exception);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Extracts the Unicode string from the byte buffer using the specified start and end indexes.
        /// </summary>
        /// <remarks>An even number of bytes is assumed between the start and ending index inclusive.</remarks>
        /// <param name="bytes">The whole collection of bytes from which to extract the Unicode string.</param>
        /// <param name="startIndexInclusive">The inclusive first byte offset from the start of the bytes to begin extraction.</param>
        /// <param name="endIndexExclusive">The exclusive last byte offset from the start of the bytes which signifies the end of the string.</param>
        /// <returns>The Unicode-encoded string.</returns>
        /// <exception cref="Exception">Thrown if an error occurs extracting the string from the bytes.</exception>
        private static string ExtractUnicodeStringFromBytes(byte[] bytes, int startIndexInclusive, int endIndexExclusive)
        {
            // VERIFY THAT THE SPECIFIED RANGE OF BYTES IS VALID.
            int stringBytesCount = (endIndexExclusive - startIndexInclusive);
            bool stringHasEvenByteCount = ((stringBytesCount % 2) == 0);
            if (!stringHasEvenByteCount)
            {
                throw new Exception("Malformed string encountered; the file cannot be read.");
            }

            // EXTRACT THE BYTES WHICH MAKE UP THE STRING.
            int bytesToSkipCount = startIndexInclusive;
            byte[] stringAsBytes = bytes.Skip(bytesToSkipCount).Take(stringBytesCount).ToArray();

            // CONVERT THE STRING BYTES INTO A UTF-16 STRING.
            string extractedString = Encoding.Unicode.GetString(stringAsBytes);
            return extractedString;
        }
        #endregion
    }
}
