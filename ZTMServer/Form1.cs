using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ZTMServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public List<string> stopsNames;
        string resources = @"..\..\Resources\";
        List<stop> stopsGPS;
        List<BusOnStop> busesOnStops;
        List<Line> allLines;
        List<Course> allCourses;
        public static dayOptions[] daysHours = {
                 new dayOptions(0, "Dni powszednie", 1,1,1,1,1,0,0),
                 new dayOptions(1, "Soboty", 0,0,0,0,0,1,0),
                 new dayOptions(2, "Niedziele i święta", 0,0,0,0,0,0,1),
                 new dayOptions(3, "Niedziele i święta oprócz", 0,0,0,0,0,0,1),
                 new dayOptions(4, "Soboty, niedziele i święta", 0,0,0,0,0,1,1),
                 new dayOptions(5, "Oprócz nocy z piątku na sobotę i z soboty na niedzielę", 2,2,2,2,0,0,2),
                 new dayOptions(6, "Noce z piątku na sobotę i z soboty na niedzielę", 0,0,0,0,2,2,0),
                 new dayOptions(7, "Codziennie", 1,1,1,1,1,1,1),
                 new dayOptions(8, "Dni powszednie - okres nauki szkolnej", 1,1,1,1,1,0,0),
                 new dayOptions(9, "Dni powszednie - ferie i wakacje", 1,1,1,1,1,0,0),
                 new dayOptions(10, "Unknown",0,0,0,0,0,0,0) };

        //main classes
        public class stop
        {
            public int id;
            public int stopId;
            public string name;
            public string lat;
            public string lon;
        }
        public class BusOnStop
        {
            public string line;
            public int lineID;
            public int stopID;
            public string time;
            public int courseID;
            public string dayOfWeek;
            public BusOnStop(string l, int s, TimeOpt t, int c)
            {
                line = l;
                stopID = s;
                time = t.toString();
                courseID = c;
            }
            public BusOnStop(int l, int s, TimeOpt t)
            {
                lineID = l;
                stopID = s;
                time = t.toString()+":00";
                dayOfWeek = t.getDay();
            }
        };
        public class Line
        {
            public string line;
            public int endStopId;
            public string allStops;
            public Line(string l, int stopId)
            {
                line = l;
                endStopId = stopId;
            }
            public bool comp(string name, int dirID)
            {
                if (name == line && dirID == endStopId)
                    return true;
                return false;
            }

        }
        public class Course
        {
            public int lineId;
            public string start;
            public string end;
            public TimeOpt start_t;
            public TimeOpt end_t;
            public int variantId;
            public Course (int l, TimeOpt s, TimeOpt e, dayOptions d)
            {
                lineId = l;
                start_t = s;
                end_t = e;
                start = s.toString();
                end = e.toString();
                variantId = d.id;
            }

        }
        public class dayOptions
        {
            public int id;
            public string name;
            public int[] days = new int[7];
            
            public dayOptions(int i, string n, int pon, int wt, int sr, int czw, int pt, int sob, int nd)
            {
                id = i;
                name = n;
                days = new int[(int)daysWeek.DAYS_WEEK];
                days[(int)daysWeek.PON] = pon;
                days[(int)daysWeek.WT] = wt;
                days[(int)daysWeek.SR] = sr;
                days[(int)daysWeek.CZW] = czw;
                days[(int)daysWeek.PT] = pt;
                days[(int)daysWeek.SOB] = sob;
                days[(int)daysWeek.ND] = nd;
                
            }

        }

        //helping classes
        public enum daysWeek
        {
            PON,
            WT,
            SR,
            CZW,
            PT,
            SOB,
            ND,
            DAYS_WEEK
        };
        public class TimeOpt
        {
            public int minutes;
            public int hours;
            public string dayOfWeek;
            public int dayOfWeekNum;

            string[] daysNames = { "Poniedziałek", "Wtorek", "Środa", "Czwartek",
                                    "Piątek", "Sobota", "Niedziela"};

            static public bool isMoring(string time)
            {
                int h = int.Parse(time.Substring(0, 2));
              
                return (h < 15);
            }

            public bool isEvening()
            {
                return (hours > 12);
            }
            public string getDay()
            {
                return daysNames[dayOfWeekNum];
            }

            public TimeOpt(int m, int h)
            {
                minutes = m;
                hours = h;
            }

            public TimeOpt(string time)
            {
                minutes = int.Parse(time.Substring(3, 2));
                hours = int.Parse(time.Substring(0, 2));
            }
            public TimeOpt(string time, int day)
            {
                minutes = int.Parse(time.Substring(3, 2));
                hours = int.Parse(time.Substring(0, 2));
                dayOfWeekNum = day;
            }
            public TimeOpt(TimeOpt time)
            {
                hours = time.hours;
                minutes = time.minutes;
            }
            public TimeOpt(TimeOpt time, int dayNum)
            {
                hours = time.hours;
                minutes = time.minutes;
                dayOfWeek = daysNames[dayNum];
                dayOfWeekNum = dayNum;
            }
            public void addMinutes(int toAdd)
            {
                minutes += toAdd;
                if (minutes >= 60)
                    hours++;
                if (hours >= 24)
                {
                    hours = 0;
                    dayOfWeekNum = (dayOfWeekNum + 1) % 7;
                }
                minutes = minutes % 60;
            }
            public string toString()
            {
                return hours + ":" + minutes;
            }

            public bool isEarlier(string firstTime)
            {
                return true;
            }
        };
        public class OneVariant
        {
            public int stopID;
            public int time;
            public OneVariant(int s, int t)
            {
                stopID = s;
                time = t;
            }
        }
        public class Variant
        {
            public int nVariant;
            public string line;
            public int lineID;
            public int dir;
            public string variant;
            public List<OneVariant> course;
            public int dirID;
            public Variant(string l, string v, int id, int d, int di, int li)
            {
                line = l;
                variant = v;
                nVariant = id;
                dir = d;
                dirID = di;
                course = new List<OneVariant>();
                lineID = li;
            }
            public void addOpt(int stop, int time)
            {
                course.Add(new OneVariant(stop, time));
            }
            public bool comp(string l, string v)
            {
                if (l == line && v == variant)
                    return true;
                return false;
            }
            public static Variant find(string l, string w, List<Variant> variants)
            {
                foreach (Variant v in variants)
                {
                    if (v.comp(l, w))
                        return v;
                }
                return null;
            }
            public void addCourse(dayOptions dO, string t, List<BusOnStop> busesOnStops, List<Course> allCourses)
            {
                for(int i = 0; i < dO.days.Length; i++)
                {
                    int day = i;
                    if (dO.days[i] == 0)
                        continue;
                    if (dO.days[i] == 2 && TimeOpt.isMoring(t))
                        day = (day + 1) % 7;
                    TimeOpt time = new TimeOpt(t, day);
                    foreach (OneVariant c in course)
                    {
                        if (c.stopID == -1)
                            System.Console.WriteLine("dupka");
                        time.addMinutes(c.time);
                        // busesOnStops.Add(new BusOnStop(line, c.stopID, time, CourseID));
                        busesOnStops.Add(new BusOnStop(lineID, c.stopID, time));
                    }
                }
            }
        }

        //helping functions
        public static dayOptions getDaysHours(string option)
        {
            option = option.Replace(".", "");
            foreach (dayOptions o in daysHours)
            {
                if (option == o.name)
                    return o;
            }
            return null;
        }

        private void getStopsOpenStreet(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            stopsGPS = new List<stop>();
            int i = 0;
                int id = 0;
            while (i < lines.Length)
            {
                string line = lines[i];
                if (line.Contains("<node") && !(line.Contains("/>")))
                {
                    string name = "";
                    bool isStop = false;
                    while (!lines[++i].Contains("</node>"))
                    {
                        if (lines[i].Contains("bus_stop") || lines[i].Contains("tram_stop") || lines[i].Contains("ref:ztm") || lines[i].Contains("stop_position"))
                            isStop = true;
                        
                        else if (lines[i].Contains("name"))
                        {
                            int start = lines[i].IndexOf("v=", 0) + 3;
                            if (start != 0)
                                name = lines[i].Substring(start, lines[i].Length - 3 - start);
                        }
                    }
                    if (isStop && name != "")
                    {
                        stop newStop = new stop();
                        newStop.id = id++;
                        name = validateName(name);
                        newStop.name = name;
                        int latPos = line.IndexOf("lat=", 0);
                        int lonPos = line.IndexOf("lon=", 0);
                        newStop.lat = line.Substring(latPos + 5, 10);
                        newStop.lon = line.Substring(lonPos + 5, 10);
                        newStop.stopId = getStopID(name);
                        stopsGPS.Add(newStop);
                    }
                }
                i++;
            }

        }

        public void uploadTables()
        {
            string startPath = "start";
            string rozkladyIn = "rozklady.zip";
            string rozklady = "rozklady";
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile("http://www.ztm.gda.pl/rozklady/pobierz_rozklady.php", rozkladyIn);
            }

            //ZipFile.CreateFromDirectory(startPath, rozkladyIn);
            string[] files = Directory.GetFiles(rozklady);
            Console.WriteLine(files);
            
        }

        public List<string> getOptsFromCSV(string line, bool isFirstLine)
        {
            List<string> opts = new List<string>();
            int id = 0;
            while(true)
            {
                bool endOfLine = false;
                int end = line.IndexOf(";", id);                
                if (end == -1)
                {
                    end = line.Length;
                    endOfLine = true;
                }
                int start = id;
                id = end + 1;

                int bracket = line.IndexOf("(", start);
                if (isFirstLine && bracket < end && bracket > start)
                    end = bracket;

                opts.Add(line.Substring(start, end - start));
                if (endOfLine)
                    break;
            }
                
            return opts;
        }

        public int getStopID(string name)
        {
            int i = 0;
            foreach (string s in stopsNames)
            {
                if (name == s)
                    return i;
                i++;
            }
            return -1;
        }

        public int getLineID(string name, int dirID)
        {
            int i = 0;
            foreach (Line l in allLines)
            {
                if (l.comp(name, dirID))
                    return i;
                i++;
            }
            return -1;
        }


        public string validateName(string name)
        {
            name = name.Replace(@" (n/ż)", "");
            name = name.Replace(@" n/ż", "");
            name = name.Replace(" (NŻ)", "");
            name = name.Replace(" NŻ", "");
            name = name.Replace("&quot;", "\"");
            name = name.Replace("&quot;", "\"");
            name = name.Replace(" (dla wysiad.)", "");
            name = name.Replace(" (dla wysiadających)", "");
            name = name.Replace("Os.", "Osiedle");
            name = name.Replace("Al.", "Aleja");
            name = name.Replace("al.", "Aleja");
            name = name.Replace(" - ", " ");
            name = name.Replace("P D P S", "PDPS");
            name = name.Replace("ARENA", "Arena");
            name = name.Replace("Gdański,", "");
            name = name.Replace("Oliva", "Oliwa");
            name = name.Replace("Pl.", "Plac");
            name = name.Replace("Mar.", "Marynarki");
            name = name.Replace("NOWY PORT", "Nowy Port");
            name = name.Replace("Gostyńska Szpital", "Gostyńska");
            
            name = name.Replace(";", "");
            name = name.Replace('¶', 'ś');
            name = name.Replace('¦', 'Ś');
            if (!(name.Contains("Sobieszewska Pastwa") || name.Contains("Dywizjonu")))
            {
                name = name.Replace("0", "");
                name = name.Replace("1", "");
                name = name.Replace("2", "");
                name = name.Replace("3", "");
                name = name.Replace("4", "");
                name = name.Replace("5", "");
                name = name.Replace("6", "");
                name = name.Replace("7", "");
                name = name.Replace("8", "");
                name = name.Replace("9", "");
            }

            if (name != "" && name[name.Length - 1] == ' ')
                name = name.Remove(name.Length - 1, 1);
            return name;
        }

        public string parseName(string name)
        {
            name = validateName(name);


            bool isDup = false;
            foreach (string tmp in stopsNames)
            {
                if (tmp == name)
                {
                    isDup = true;
                    break;
                }
            }
            if (!isDup)
                stopsNames.Add(name);
            return name;
        }


        //functions for managing buttons

        //get data from OpenStreetMap
        private void button1_Click(object sender, EventArgs e)
        {
            string output = textBox1.Text;
            textBox1.Text = output + "Pobieram dane z OpenStreetMap\n";
            getStopsOpenStreet(resources + @"maps\map");
            textBox1.Text = output + "Pobrano dane z OpenStreetMap" + Environment.NewLine;
        }

        private void getDatafromZTM()
        {
            string path = resources + "rozklady";
            string[] dirs = Directory.GetDirectories(path);
            path = dirs[0];
            dirs = Directory.GetDirectories(path);
            stopsNames = new List<string>();
            busesOnStops = new List<BusOnStop>();
            allLines = new List<Line>();
            allCourses = new List<Course>();
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                List<string> warianty = new List<string>();
                List<string> kurs = new List<string>();
                List<Variant> variants = new List<Variant>();
                foreach (string file in files)
                {
                    if (file.Contains("warianty"))
                        warianty.Add(file);
                    else if (file.Contains("kursy"))
                        kurs.Add(file);
                }
                //   List<string> stopsToFile = new List<string>();
                string stopsToFile = "";
                foreach (string file in warianty)
                {
                    int direction = int.Parse(file.Substring(file.Length - 5, 1));
                    string[] lines = System.IO.File.ReadAllLines(file, Encoding.Default);
                    List<string> opts;
                    string lineNum = getOptsFromCSV(lines[0], true)[0];
                    if (lineNum == "110")
                        opts = null;

                    List<string> lastLineopts = getOptsFromCSV(lines[lines.Length - 1], false);
                    string lastName = lastLineopts[3];
                    lastName = parseName(lastName);
                    int dirID = getStopID(lastName);
                    allLines.Add(new Line(lineNum, dirID));
                    foreach (string line in lines)
                    {
                        if (line == lines[0])
                        {
                            opts = getOptsFromCSV(line, true);
                            bool isVariant = false;
                            int i = 0;

                            foreach (string opt in opts)
                            {
                                if (isVariant)
                                    variants.Add(new Variant(lineNum, opt, i, direction, dirID, getLineID(lineNum, dirID)));
                                if (opt.Contains("Nazwa"))
                                    isVariant = true;
                                i++;
                            }
                            continue;
                        }
                        opts = getOptsFromCSV(line, false);
                        string name = parseName(opts[3]);
                        //  stopsToFile.Add(name);
                        stopsToFile += name + ",";
                        foreach (Variant v in variants)
                        {
                            if (v.dir == direction && v.nVariant < opts.Count && opts[v.nVariant] != "")
                            {
                                v.addOpt(getStopID(name), int.Parse(opts[v.nVariant]));

                            }
                        }
                    }
                    //pliczek z trasa zapisany
                    // File.WriteAllLines(resources + @"stops\line" + lineNum + (allLines.Count - 1), stopsToFile);
                    allLines[allLines.Count - 1].allStops = stopsToFile;
                    stopsToFile = "";
                }
                foreach (string file in kurs)
                {
                    int direction = int.Parse(file.Substring(file.Length - 5, 1));
                    string lineNum = Path.GetFileName(file).Substring(0, 3);
                    for (int i = 0; i < 2; i++)
                    {
                        if (lineNum[0] == '0')
                            lineNum = lineNum.Remove(0, 1);
                    }
                    dayOptions dO = null;
                    string[] lines = System.IO.File.ReadAllLines(file, Encoding.Default);
                    foreach (string line in lines)
                    {
                        List<string> opts = getOptsFromCSV(line, true);
                        dayOptions tmp = getDaysHours(opts[1]);
                        if (tmp != null)
                        {
                            dO = tmp;
                            continue;
                        }
                        Variant v = Variant.find(lineNum, opts[1], variants);
                        v.addCourse(dO, opts[0], busesOnStops, allCourses);
                    }
                }
            }
        }
      
        //get data from zip uploaded from ZTM
        private void button3_Click(object sender, EventArgs e)
        {
            string output = textBox1.Text;
            textBox1.Text = output + "Pobieram dane ze strony ZTM" + Environment.NewLine;
            getDatafromZTM();
            textBox1.Text = output + "Pobrano dane ze strony ZTM" + Environment.NewLine;
        }

        //put GPS to database
        private void button4_Click(object sender, EventArgs e)
        {
            string output = textBox1.Text + "Wrzucam StopGPS do bazy: ";
            MySqlConnection connection = new MySqlConnection("SERVER=s12.hekko.net.pl;PORT=3306;DATABASE=ztmobile_0;UID=ztmobile_0;PWD=admin123;");
            MySqlCommand command;
            string query;
            Boolean result;

            foreach (stop s in stopsGPS)
            {
                try
                {
                    connection.Open();
                    query = "INSERT INTO StopGPS(ID,Lat,Lon,Name,StopID) VALUES(@id,@lat,@lon,@name,@stopId)";

                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", s.id);
                    command.Parameters.AddWithValue("@lat", double.Parse(s.lat, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo));
                    command.Parameters.AddWithValue("@lon", double.Parse(s.lon, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo));
                    command.Parameters.AddWithValue("@name", s.name);
                    command.Parameters.AddWithValue("@stopId", s.stopId);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    result = true;

                    textBox1.Text = output + stopsGPS.IndexOf(s) + "/" + stopsGPS.Count + "\n";
                }
                catch (Exception ex)
                {
                    textBox1.Text = "Błąd podczas wrzucania do tabeli StopGPS";
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
                if (!result)
                    return;
            }
        }

        //put stops to database
        private void button6_Click(object sender, EventArgs e)
        {

            string output = textBox1.Text + "Wrzucam Stops do bazy: ";

            MySqlConnection connection = new MySqlConnection("SERVER=s12.hekko.net.pl;PORT=3306;DATABASE=ztmobile_0;UID=ztmobile_0;PWD=admin123;");
            MySqlCommand command;
            string query;
            Boolean result;

            foreach (string s in stopsNames)
            {
                try
                {
                    connection.Open();
                    query = "INSERT INTO Stops(ID,Name) VALUES(@id,@name)";

                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", stopsNames.IndexOf(s));
                    command.Parameters.AddWithValue("@name", s);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    result = true;

                    textBox1.Text = output + stopsNames.IndexOf(s) + "/" + stopsNames.Count + "\n";
                }
                catch (Exception ex)
                {
                    textBox1.Text = "Błąd podczas wrzucania do tabeli stpos";
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
                if (!result)
                    return;
            }
        }

        //put lines to database
        private void button7_Click(object sender, EventArgs e)
        {
            string output = textBox1.Text + "Wrzucam BusLines do bazy: ";

            MySqlConnection connection = new MySqlConnection("SERVER=s12.hekko.net.pl;PORT=3306;DATABASE=ztmobile_0;UID=ztmobile_0;PWD=admin123;");
            MySqlCommand command;
            string query;
            Boolean result;

            foreach (Line l in allLines)
            {
                try
                {
                    connection.Open();
                    query = "INSERT INTO BusLines(ID,Name,Direction,DirID,AllStops) VALUES(@id,@name,@dir,@dirID,@file)";

                    string filePath = resources + @"stops\line" + l.line + allLines.IndexOf(l);
                    MemoryStream stream = new MemoryStream();
                    byte[] byte_arr = System.IO.File.ReadAllBytes(filePath);
                    String file_str = System.Convert.ToBase64String(byte_arr);

                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", allLines.IndexOf(l));
                    command.Parameters.AddWithValue("@name", l.line);
                    command.Parameters.AddWithValue("@dir", stopsNames[l.endStopId]);
                    command.Parameters.AddWithValue("@dirID", l.endStopId);
                    command.Parameters.AddWithValue("@file", l.allStops);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    result = true;
                    textBox1.Text = output + allLines.IndexOf(l) + "/" + allLines.Count + "\n";
                }
                catch (Exception ex)
                {
                    textBox1.Text = "Błąd podczas wrzucania do tabeli BusLines";
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
                if (!result)
                    return;
            }

        }

        //put all table to database
        private void button8_Click(object sender, EventArgs e)
        {
            string output = textBox1.Text + "Wrzucam BusOnStop do bazy: ";

            MySqlConnection connection = new MySqlConnection("SERVER=s12.hekko.net.pl;PORT=3306;DATABASE=ztmobile_0;UID=ztmobile_0;PWD=admin123;");
            MySqlCommand command;
            string query;
            Boolean result;

            foreach (BusOnStop b in busesOnStops)
            {
                    try
                    {
            //            connection.Open();
           //             query = "INSERT INTO BusOnStop(LineID,StopID,Hour,Day) VALUES(@line,@stop,@hour,@day)";


           //             command = new MySqlCommand(query, connection);
            //            command.Parameters.AddWithValue("@line", b.lineID);
              //          command.Parameters.AddWithValue("@stop", b.stopID);
                //        command.Parameters.AddWithValue("@hour", b.time);
                  //      command.Parameters.AddWithValue("@day", b.dayOfWeek);
                    //    command.ExecuteNonQuery();
                      //  command.Parameters.Clear();
                        result = true;
                    textBox1.Text = output + busesOnStops.IndexOf(b) + "/" + busesOnStops.Count + "\n";
                    }
                    catch (Exception ex)
                    {
                        textBox1.Text = "Błąd podczas wrzucania do tabeli BusOnStop";
                        result = false;
                    }
                    finally
                    {
                        //connection.Close();
                    }
                    if (!result)
                        return;
                
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<string> names = new List<String>();
            foreach (string s in stopsNames)
            {
                bool someFlag = false;
                foreach (stop g in stopsGPS)
                {
                    if (s == g.name)
                    {
                        someFlag = true;
                        break;
                    }
                }
                if (!someFlag)
                    names.Add(s);
            }
            File.WriteAllLines(resources + @"testy\namesCounter", names);


            List<string> gpses = new List<String>();
            foreach (stop g in stopsGPS)
            {
                bool someFlag = false;
                foreach (string s in stopsNames)
                {
                    if (s == g.name)
                    {
                        someFlag = true;
                        break;
                    }
                }
                if (!someFlag)
                    gpses.Add(g.name);
            }
            File.WriteAllLines(resources + @"testy\gpsCounter", gpses);
        }
        
        private void button2_Click_1(object sender, EventArgs e)
        {

            getDatafromZTM();
            getStopsOpenStreet(resources + @"maps\map");
            button4_Click(sender, e);
            button6_Click(sender, e);
            button7_Click(sender, e);
            button8_Click(sender, e);
        }
    }
}
