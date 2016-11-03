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
            public int stopID;
            public string time;
            public int courseID;
            public BusOnStop(string l, int s, TimeOpt t, int c)
            {
                line = l;
                stopID = s;
                time = t.toString();
                courseID = c;
            }
        };
        public class Line
        {
            public string line;
            public int endStopId;
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

            string[] daysNames = { "poniedzialek", "wtorek", "sroda", "czwartek",
                                    "piatek", "sobota", "niedziela"};
            public bool isEvening()
            {
                return (hours > 12);
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
                }
                minutes = minutes % 60;
            }
            public string toString()
            {
                return hours + ":" + minutes;
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
                TimeOpt time = new TimeOpt(t);
               
                foreach (OneVariant c in course)
                    time.addMinutes(c.time);

              
                allCourses.Add(new Course(lineID, new TimeOpt(t), new TimeOpt(time), dO));
                int CourseID = allCourses.Count - 1;

                foreach (OneVariant c in course)
                {
                    time.addMinutes(c.time);
                    busesOnStops.Add(new BusOnStop(line, c.stopID, time, CourseID));
                }


            }

        }

        //helping functions
        public static dayOptions getDaysHours(string option)
        {
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

        public void parseName(string name)
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
        }


        //functions for managing buttons
        //get data from OpenStreetMap
        private void button1_Click(object sender, EventArgs e)
        {
            getStopsOpenStreet(@"maps\mapMedium");
        }

        //get data from zip uploaded from ZTM
        private void button3_Click(object sender, EventArgs e)
        {
            string path = @"rozklady";
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
                foreach (string file in warianty)
                { 
                    int direction = int.Parse(file.Substring(file.Length - 5, 1));
                    string[] lines = System.IO.File.ReadAllLines(file, Encoding.Default);
                    List<string> opts;
                    string lineNum = getOptsFromCSV(lines[0], true)[0];

                    List<string> lastLineopts = getOptsFromCSV(lines[lines.Length - 1], false);
                    string lastName = lastLineopts[3];
                    parseName(lastName);
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
                        string name = opts[3];
                        parseName(name);
                        foreach (Variant v in variants)
                        {
                            if (v.nVariant < opts.Count && opts[v.nVariant] != "") 
                                v.addOpt(getStopID(name), int.Parse(opts[v.nVariant]));
                        }
                    }
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

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllLines(@"..\Resources\testy\stopNames", stopsNames);

            List<string> justAllLines = new List<string>();
            for (int i = 0; i < stopsNames.Count; i++)
                justAllLines.Add(i + stopsNames[i]);
            File.WriteAllLines(@"..\Resources\testy\stopsGPS", justAllLines);
            justAllLines = new List<string>();
            foreach (stop b in stopsGPS)
                justAllLines.Add(stopsGPS.IndexOf(b) + "" + double.Parse(b.lat, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
            + "" + double.Parse(b.lon, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
            + b.name + b.stopId);
            File.WriteAllLines(@"..\Resources\testy\stopsGPS", justAllLines);

            justAllLines = new List<string>();
            foreach (dayOptions b in daysHours)
            {
                string days = "";
                foreach (int d in b.days)
                    days += d + "-";
                justAllLines.Add(b.id + b.name + days);

            }
            File.WriteAllLines(@"..\Resources\testy\variants", justAllLines);

            justAllLines = new List<string>();
            foreach (Line b in allLines)
                justAllLines.Add(allLines.IndexOf(b) + "" + b.line + "" + b.endStopId);
            File.WriteAllLines(@"..\Resources\testy\lines", justAllLines);

            justAllLines = new List<string>();
            foreach (Course b in allCourses)
                justAllLines.Add(allCourses.IndexOf(b) + "" + b.lineId + "" + b.start + b.end);
            File.WriteAllLines(@"..\Resources\testy\courses", justAllLines);

            justAllLines = new List<string>();
            foreach (BusOnStop b in busesOnStops)
                justAllLines.Add(busesOnStops.IndexOf(b) + "" + b.courseID + "" + b.stopID + "" + b.time);
            File.WriteAllLines(@"..\Resources\testy\busesOnStops", justAllLines);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MySqlConnection connection = new MySqlConnection("SERVER=db4free.net;PORT=3306;DATABASE=ztmobile;UID=ztmobile;PWD=admin123;");
            MySqlCommand command;
            string query;
            bool result;

            foreach (stop s in stopsGPS)
            {
                try
                {
                    connection.Open();
                    query = "INSERT INTO GPS(ID,Name,Lat,Lon,STOP_ID) VALUES(@id,@name,@lat,@lon,@stopId)";

                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", s.id);
                    command.Parameters.AddWithValue("@name", s.name);
                    command.Parameters.AddWithValue("@lat", double.Parse(s.lat, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo));
                    command.Parameters.AddWithValue("@lon", double.Parse(s.lon, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo));
                    command.Parameters.AddWithValue("@stopId", s.stopId);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    result = true;
                }
                catch (Exception ex)
                {
                    label1.Text = "Błąd";
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
                if (!result)
                    return;
                label1.Text = "Udalo sie";

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
            File.WriteAllLines(@"..\Resources\testy\namesCounter", names);


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
            File.WriteAllLines(@"..\Resources\testy\gpsCounter", gpses);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
