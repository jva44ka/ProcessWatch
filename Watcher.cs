using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessWatcher
{
    /*Делегат нужен чтобы в зависимости от того удалось создать файл или нет записывать 
     * в него данные или только в консоль*/
    internal delegate void DelegateMessage(string text);

    /// <summary>
    /// Сущность, следящая за процессом и убивающая его
    /// </summary>
    public class Watcher
    {
        //Имя процесса, за которым нужно следить
        private string processName;
        //Наш процесс
        private Process process;
        //Все процессы
        private Process[] processes;
        //Время допустимой жизни (в минутах)
        private byte aliveTime;
        //Интервал проверки (в минутах)
        private byte checkTime;
        //Название метода для записи
        private DelegateMessage writer;
        //Поток для записи логов в файл
        private StreamWriter logStream;

        /// <param name="processName">Имя процесса, за которым нужно следить</param>
        /// <param name="aliveTime">Время допустимой жизни (в минутах)</param>
        /// <param name="checkTime">Интервал проверки (в минутах)</param>
        public Watcher(string processName = "Spotify", string aliveTime = "1", string checkTime = "1")
        {
            this.processName = processName;

            bool convertResult1 = Byte.TryParse(aliveTime, out this.aliveTime);
            bool convertResult2 = Byte.TryParse(checkTime, out this.checkTime);

            if (convertResult1 && convertResult2)
            {
                this.InitStream();
                writer.Invoke("Наблюдение запущено успешно.");
                this.Watch();
            }
            else
            {
                writer.Invoke("Введены некорректные данные.");
            }
        }

        //~Watcher()
        //{
        //    logStream.Dispose();
        //}

        //Для записи логов в консоль и файл
        private void WriteWithFile(string text)
        {
            Console.WriteLine("{0:t}\t{1}", DateTime.Now, text);
            this.logStream.WriteLine("{0}\t{1}", DateTime.Now, text);
        }

        //Для записи логов в консоль
        private void WriteWithoutFile(string text)
        {
            Console.WriteLine("{0:t}\t{1}", DateTime.Now, text);
        }

        //Для инициализации потока вывода в файл
        private void InitStream(string way = "log.txt")
        {
            try
            {
                this.logStream = new StreamWriter(way);
            }
            catch (IOException ex)
            {
                writer = this.WriteWithoutFile;
                writer.Invoke(String.Format("Не удалось создать лог-файл: {0}", ex.Message));
            }
            finally
            {
                writer = this.WriteWithFile;
                writer.Invoke("Файл с логами создан.");
            }
        }

        //Наблюдение
        private void Watch()
        {
            try
            {
                processes = Process.GetProcessesByName(this.processName);
                if (processes.Length == 0)
                    writer.Invoke("Процесс не найден.");
                byte watchingMinutes = 0;

                while (processes.Length != 0)
                {
                    if (watchingMinutes >= this.aliveTime)
                    {
                        this.Kill();
                        writer.Invoke("Процесс завершен принудительно.");
                        break;
                    }

                    Thread.Sleep(60000 * this.checkTime);
                    watchingMinutes++;
                    writer.Invoke(String.Format("Процесс в работе уже {0} из {1} минут.",
                        watchingMinutes, this.aliveTime));
                    processes = Process.GetProcessesByName(this.processName);
                }
            }
            catch (Exception ex)
            {
                writer.Invoke(String.Format("Исключение при наблюдении: {0}", ex.Message));
            }
            finally
            {
                writer.Invoke("Наблюдение завершено");
            }
        }

        //Убийство процесса
        private void Kill()
        {
            foreach (Process item in this.processes)
            {
                item.Kill();
            }
        }
    }
}
