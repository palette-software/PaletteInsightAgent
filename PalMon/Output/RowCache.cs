using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    class RowCache<T> where T : class
    {
        private const int FlushTimeInSeconds = 10;
        private Queue<T> queue;

        private string fileBaseName;

        /// <summary>
        /// On each flush, this delegate will be called with the CSV file name and the objects
        /// in the queue before the flush.
        /// 
        /// Upon returning, the queue is cleared
        /// </summary>
        Action<string, T[]> onFlushDelegate;


        public RowCache(string baseName, Action<string,  T[]> onFlush)
        {
            fileBaseName = baseName;
            onFlushDelegate = onFlush;
            engine = new FileHelpers.FileHelperEngine<T>();
            queue = new Queue<T>();

            lastOutputDate = DateTimeOffset.MinValue;

        }

        public void Put(T[] rows)
        {
            // start a new output if we need to and flush the existing output
            var newFileStarted = StartNewFileIfNeeded();

            // add all rows to the queue
            foreach (var row in rows)
            {
                queue.Enqueue(row);
            }
        }


        /// <summary>
        /// Starts a new file if needed and tries to write the results to the database
        /// </summary>
        public void FlushCacheIfNeeded()
        {
            StartNewFileIfNeeded();
        }

        #region CSV output

        private DateTimeOffset lastOutputDate;
        private DateTimeOffset nextOutputDate;

        /// <summary>
        /// The CSV serialization engine
        /// </summary>
        private FileHelpers.FileHelperEngine<T> engine;

        /// <summary>
        /// Starts a new CSV file
        /// </summary>
        private void StartNewFile()
        {
            // If there is no last output time is set, and the queue is not empty, we have trouble
            if (lastOutputDate == DateTimeOffset.MinValue && !(queue.Count == 0))
            {
                throw new ArgumentNullException("No CSV filename set while the queue is not empty!");
            }

            // Output the existing data to CSV if we need to
            if (lastOutputDate != DateTimeOffset.MinValue)
            {
                // get a new filename
                var lastFileName = String.Format("{0}-{1:yyyy-MM-dd--HH-mm-ss}.csv", fileBaseName, lastOutputDate.UtcDateTime);

                // try to create the directory of the output
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(lastFileName));
                }
                catch (Exception e)
                {
                    // Do nada
                }

                // make sure we output headers
                engine.HeaderText = engine.GetFileHeader();

                // flush the queue
                // TODO: use FileHelperAsyncEngine instead for faster writing
                engine.WriteFile(lastFileName, queue);

                // Call the flush function with the data in the queue
                onFlushDelegate(lastFileName, queue.ToArray());

                // after the flush delegate is called and the CSV is written, clear our queue
                queue.Clear();
            }

            // set up the output timestamps
            lastOutputDate = DateTimeOffset.UtcNow;
            nextOutputDate = lastOutputDate.AddSeconds(FlushTimeInSeconds);
        }


        /// <summary>
        /// Only start a new file if we need to
        /// </summary>
        /// <returns></returns>
        private bool StartNewFileIfNeeded()
        {
            // if either the last or next update date is null or the time is over the next output date
            var needsNewFile = (lastOutputDate == null) || (nextOutputDate == null) || (DateTimeOffset.UtcNow >= nextOutputDate);

            if (!needsNewFile) return false;

            StartNewFile();
            return true;
        }
        #endregion



    }
}
