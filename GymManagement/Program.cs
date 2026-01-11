using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymManagement
{
   
    public enum SubscriptionType
    {
        Lunar,
        Anual
    }

    public enum ClassType
    {
        Fitness,
        Pilates,
        Yoga
    }

    public class Trainer
    {
        public string Name { get; set; }
        public string Specialization { get; set; }

        public Trainer(string name, string specialization)
        {
            Name = name;
            Specialization = specialization;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"    Antrenor: {Name}");
            Console.WriteLine($"    Specializare: {Specialization}");
        }
    }

    public class ClassSession
    {
        public string Id { get; set; }
        public ClassType Type { get; set; }
        public Trainer Trainer { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public List<string> Participants { get; set; }
        public DateTime StartTime { get; set; }
        public string RoomLocation { get; set; }

        public ClassSession(ClassType type, Trainer trainer, int durationMinutes, int maxCapacity, DateTime startTime,
            string roomLocation)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Type = type;
            Trainer = trainer;
            DurationMinutes = durationMinutes;
            MaxCapacity = maxCapacity;
            Participants = new List<string>();
            StartTime = startTime;
            RoomLocation = roomLocation;
        }

        public bool CanReserve()
        {
            return Participants.Count < MaxCapacity;
        }

        public bool Reserve(string username)
        {
            if (Participants.Contains(username))
                return false;

            if (!CanReserve())
                return false;

            Participants.Add(username);
            return true;
        }

        public bool CancelReservation(string username)
        {
            return Participants.Remove(username);
        }

        public double GetOccupancyRate()
        {
            return MaxCapacity > 0 ? (double)Participants.Count / MaxCapacity * 100 : 0;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n  [{Id}] Clasă {Type}");
            Console.WriteLine($"    Data/Ora: {StartTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"    Durată: {DurationMinutes} minute");
            Console.WriteLine($"    Locație: {RoomLocation}");
            Trainer.DisplayInfo();
            Console.WriteLine($"    Locuri: {Participants.Count}/{MaxCapacity} ({GetOccupancyRate():F1}% ocupat)");
            Console.WriteLine($"    Status: {(CanReserve() ? "DISPONIBIL" : "COMPLET")}");
        }
    }

    public class Subscription
    {
        public SubscriptionType Type { get; set; }
        public decimal Price { get; set; }
        public List<string> Benefits { get; set; }
        public int DurationDays { get; set; }

        public Subscription(SubscriptionType type)
        {
            Type = type;
            Benefits = new List<string>();

            if (type == SubscriptionType.Lunar)
            {
                Price = 150;
                DurationDays = 30;
                Benefits.Add("Acces la toate zonele");
                Benefits.Add("1 sesiune de evaluare gratuită");
                Benefits.Add("Dus și vestiare");
            }
            else
            {
                Price = 1500;
                DurationDays = 365;
                Benefits.Add("Acces la toate zonele");
                Benefits.Add("2 sesiuni cu antrenor personal/lună");
                Benefits.Add("Dus și vestiare");
                Benefits.Add("Acces prioritar în orele de vârf");
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n--- ABONAMENT {Type.ToString().ToUpper()} ---");
            Console.WriteLine($"Preț: {Price} RON");
            Console.WriteLine($"Durată: {DurationDays} zile");
            Console.WriteLine("Beneficii:");
            foreach (var benefit in Benefits)
            {
                Console.WriteLine($"  ✓ {benefit}");
            }
        }
    }

    public class Zone
    {
        public string Name { get; set; }
        public string Schedule { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }

        public Zone(string name, string schedule, int maxCapacity)
        {
            Name = name;
            Schedule = schedule;
            MaxCapacity = maxCapacity;
            CurrentOccupancy = 0;
        }

        public bool CanEnter()
        {
            return CurrentOccupancy < MaxCapacity;
        }

        public bool CheckIn()
        {
            if (CanEnter())
            {
                CurrentOccupancy++;
                return true;
            }

            return false;
        }

        public void CheckOut()
        {
            if (CurrentOccupancy > 0)
                CurrentOccupancy--;
        }

        public double GetOccupancyRate()
        {
            return MaxCapacity > 0 ? (double)CurrentOccupancy / MaxCapacity * 100 : 0;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n{Name}");
            Console.WriteLine($"  Program: {Schedule}");
            Console.WriteLine($"  Capacitate: {CurrentOccupancy}/{MaxCapacity} ({GetOccupancyRate():F1}%)");
            Console.WriteLine($"  Status: {(CanEnter() ? "Disponibil" : "Complet")}");
        }
    }

    public class FitnessRoom
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Dictionary<string, Zone> Zones { get; set; }
        public List<ClassSession> Classes { get; set; }

        public FitnessRoom(string name, string address)
        {
            Name = name;
            Address = address;
            Zones = new Dictionary<string, Zone>();
            Classes = new List<ClassSession>();
            InitializeZones();
        }

        private void InitializeZones()
        {
            Zones.Add("Cardio", new Zone("Zona Cardio", "06:00 - 12:00", 25));
            Zones.Add("Forta", new Zone("Zona Forță", "12:00 - 19:00", 20));
            Zones.Add("Tractiuni", new Zone("Zona Tracțiuni", "18:00 - 23:00", 15));
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"  {Name}");
            Console.WriteLine($"  Adresă: {Address}");
            foreach (var zone in Zones.Values)
            {
                zone.DisplayInfo();
            }
        }

        public void DisplayClasses()
        {
            Console.WriteLine($"  CLASE DISPONIBILE - {Name}");
            if (Classes.Count == 0)
            {
                Console.WriteLine("  Nu există clase programate.");
                return;
            }

            foreach (var classSession in Classes.OrderBy(c => c.StartTime))
            {
                classSession.DisplayInfo();
            }
        }

        public Zone GetZone(string zoneName)
        {
            return Zones.ContainsKey(zoneName) ? Zones[zoneName] : null;
        }

        public ClassSession GetClass(string classId)
        {
            return Classes.FirstOrDefault(c => c.Id == classId);
        }

        public void AddClass(ClassSession classSession)
        {
            Classes.Add(classSession);
        }

        public bool RemoveClass(string classId)
        {
            var classToRemove = GetClass(classId);
            if (classToRemove != null)
            {
                Classes.Remove(classToRemove);
                return true;
            }

            return false;
        }
    }

    public abstract class User
    {
        private string _password;
        public string Username { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        protected User(string username, string password, string fullname)
        {
            Username = username;
            _password = HashPassword(password);
            FullName = fullname;
            CreatedAt = DateTime.Now;
        }

        protected User()
        {
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public bool VerifyPassword(string password)
        {
            return _password == HashPassword(password);
        }

        public (bool success, string message) ChangePassword(string oldPassword, string newPassword)
        {
            if (!VerifyPassword(oldPassword))
                return (false, "Parola veche incorectă!");

            _password = HashPassword(newPassword);
            return (true, "Parola a fost schimbată cu succes!");
        }

        public string GetPasswordHash() => _password;
        public void SetPasswordHash(string hash) => _password = hash;
        public abstract Dictionary<string, object> GetData();
        public abstract string GetUserType();
    }

    public class Client : User
    {
        public string MembershipStatus { get; set; }
        public DateTime? MembershipExpiry { get; set; }
        public SubscriptionType? CurrentSubscription { get; set; }
        public List<DateTime> Visits { get; set; }
        public string CurrentZone { get; set; }
        public string PreferredRoom { get; set; }
        public List<string> ReservedClasses { get; set; }

        public Client(string username, string password, string fullname)
            : base(username, password, fullname)
        {
            MembershipStatus = "inactiv";
            MembershipExpiry = null;
            CurrentSubscription = null;
            Visits = new List<DateTime>();
            CurrentZone = null;
            PreferredRoom = null;
            ReservedClasses = new List<string>();
        }

        public Client() : base()
        {
            Visits = new List<DateTime>();
            ReservedClasses = new List<string>();
        }

        public string ActivateMembership(SubscriptionType type)
        {
            var subscription = new Subscription(type);
            MembershipStatus = "activ";
            CurrentSubscription = type;
            MembershipExpiry = DateTime.Now.AddDays(subscription.DurationDays);

            return
                $"Abonament {type} activat!\nPreț: {subscription.Price} RON\nValabil până la: {MembershipExpiry:yyyy-MM-dd}";
        }

        public string AddVisit()
        {
            if (MembershipStatus != "activ")
                return "Abonamentul nu este activ!";

            if (MembershipExpiry < DateTime.Now)
            {
                MembershipStatus = "expirat";
                return "Abonamentul a expirat!";
            }

            DateTime visitDate = DateTime.Now;
            Visits.Add(visitDate);
            return $"Vizită înregistrată la {visitDate:yyyy-MM-dd HH:mm:ss}";
        }

        public string CheckInToZone(FitnessRoom room, string zoneName)
        {
            if (MembershipStatus != "activ")
                return "Nu ai un abonament activ!";

            if (CurrentZone != null)
                return $"Ești deja în {CurrentZone}. Te rog să ieși mai întâi.";

            var zone = room.GetZone(zoneName);
            if (zone == null)
                return "Zona nu există!";

            if (zone.CheckIn())
            {
                CurrentZone = zoneName;
                PreferredRoom = room.Name;
                return $"Check-in efectuat în {zone.Name} la {room.Name}!";
            }

            return $"{zone.Name} este complet. Capacitate: {zone.CurrentOccupancy}/{zone.MaxCapacity}";
        }

        public string CheckOutFromZone(FitnessRoom room)
        {
            if (CurrentZone == null)
                return "Nu ești în nicio zonă!";

            var zone = room.GetZone(CurrentZone);
            if (zone != null)
            {
                zone.CheckOut();
                string message = $"Check-out efectuat din {zone.Name}!";
                CurrentZone = null;
                return message;
            }

            return "Eroare la check-out!";
        }

        public override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "username", Username },
                { "fullname", FullName },
                { "membership_status", MembershipStatus },
                { "subscription_type", CurrentSubscription?.ToString() ?? "N/A" },
                { "membership_expiry", MembershipExpiry?.ToString("yyyy-MM-dd") ?? "N/A" },
                { "total_visits", Visits.Count },
                { "reserved_classes", ReservedClasses.Count },
                { "current_zone", CurrentZone ?? "N/A" },
                { "preferred_room", PreferredRoom ?? "N/A" },
                { "created_at", CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }

        public override string GetUserType() => "Client";
        
        public string CancelMembership()
        {
            if (MembershipStatus == "inactiv")
                return "Nu ai un abonament activ de anulat!";
        
            var oldStatus = MembershipStatus;
            var oldType = CurrentSubscription;
            
            MembershipStatus = "anulat";
            MembershipExpiry = DateTime.Now;
            CurrentSubscription = null;
        
            return $"Abonamentul a fost anulat cu succes!\nPoți reactiva oricând un abonament nou.";
        }
        
    }

    public class Admin : User
    {
        public string AccessLevel { get; set; }

        public Admin(string username, string password, string fullname, string accessLevel = "standard")
            : base(username, password, fullname)
        {
            AccessLevel = accessLevel;
        }

        public Admin() : base()
        {
        }

        public override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "username", Username },
                { "fullname", FullName },
                { "access_level", AccessLevel },
                { "role", "Administrator" },
                { "created_at", CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }

        public override string GetUserType() => "Admin";
    }

    public class UserData
    {
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserType { get; set; }
        public string MembershipStatus { get; set; }
        public DateTime? MembershipExpiry { get; set; }
        public string CurrentSubscription { get; set; }
        public List<DateTime> Visits { get; set; }
        public string CurrentZone { get; set; }
        public string PreferredRoom { get; set; }
        public List<string> ReservedClasses { get; set; }
        public string AccessLevel { get; set; }
    }
    public class GymData
    {
        public Dictionary<string, UserData> Users { get; set; }
        public Dictionary<string, List<ClassSessionData>> RoomClasses { get; set; }
    
        public GymData()
        {
            Users = new Dictionary<string, UserData>();
            RoomClasses = new Dictionary<string, List<ClassSessionData>>();
        }
    }
    public class ClassSessionData
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string TrainerName { get; set; }
        public string TrainerSpecialization { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public List<string> Participants { get; set; }
        public DateTime StartTime { get; set; }
        public string RoomLocation { get; set; }
    }
    public class FileManager
    {
        private readonly string _filename;

        public FileManager(string filename = "users.json")
        {
            _filename = filename;
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_filename))
            {
                var emptyData = new GymData();
                SaveGymData(emptyData);
            }
        }
        public GymData LoadGymData()
        {
            string json = File.ReadAllText(_filename);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<GymData>(json, options) ?? new GymData();
        }

        public void SaveGymData(GymData data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_filename, json);
        }
        public Dictionary<string, UserData> LoadData()
        {
            return LoadGymData().Users;
        }

        public void SaveData(Dictionary<string, UserData> users)
        {
            var gymData = LoadGymData();
            gymData.Users = users;
            SaveGymData(gymData);
        }
    }

    public class UserRepository
    {
        private readonly FileManager _fileManager;

        public UserRepository(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public (bool success, string message) AddUser(User user)
        {
            var users = _fileManager.LoadData();

            if (users.ContainsKey(user.Username))
                return (false, "Username-ul există deja!");

            users[user.Username] = ConvertToUserData(user);
            _fileManager.SaveData(users);
            Console.WriteLine($"[DEBUG] User {user.Username} actualizat. Status: {(user is Client c ? c.MembershipStatus : "N/A")}");
            return (true, "Utilizator adăugat cu succes!");
        }

        public User GetUser(string username)
        {
            var users = _fileManager.LoadData();
            if (!users.ContainsKey(username))
                return null;
            return ConvertToUser(username, users[username]);
        }

        public (bool success, string message) UpdateUser(User user)
        {
            var users = _fileManager.LoadData();
            if (!users.ContainsKey(user.Username))
                return (false, "Utilizatorul nu există!");

            users[user.Username] = ConvertToUserData(user);
            _fileManager.SaveData(users);
            return (true, "Utilizator actualizat cu succes!");
        }

        public Dictionary<string, UserData> GetAllUsers()
        {
            return _fileManager.LoadData();
        }

        private UserData ConvertToUserData(User user)
        {
            var data = new UserData
            {
                Password = user.GetPasswordHash(),
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                UserType = user.GetUserType()
            };
            if (user is Client client)
            {
                data.MembershipStatus = client.MembershipStatus;
                data.MembershipExpiry = client.MembershipExpiry;
                data.CurrentSubscription = client.CurrentSubscription?.ToString();
                data.Visits = client.Visits;
                data.CurrentZone = client.CurrentZone;
                data.PreferredRoom = client.PreferredRoom;
                data.ReservedClasses = client.ReservedClasses;
                
                Console.WriteLine($"[DEBUG] Convertire la UserData: Status={data.MembershipStatus}, Subscription={data.CurrentSubscription}");
            }
            else if (user is Admin admin)
            {
                data.AccessLevel = admin.AccessLevel;
            }

            return data;
        }

        private User ConvertToUser(string username, UserData data)
        {
            User user;

            if (data.UserType == "Admin")
            {
                user = new Admin
                {
                    Username = username,
                    FullName = data.FullName,
                    CreatedAt = data.CreatedAt,
                    AccessLevel = data.AccessLevel ?? "standard"
                };
            }
            else
            {
                user = new Client
                {
                    Username = username,
                    FullName = data.FullName,
                    CreatedAt = data.CreatedAt,
                    MembershipStatus = data.MembershipStatus ?? "inactiv",
                    MembershipExpiry = data.MembershipExpiry,
                    CurrentSubscription = !string.IsNullOrEmpty(data.CurrentSubscription)
                        ? (SubscriptionType)Enum.Parse(typeof(SubscriptionType), data.CurrentSubscription)
                        : (SubscriptionType?)null,
                    Visits = data.Visits ?? new List<DateTime>(),
                    CurrentZone = data.CurrentZone,
                    PreferredRoom = data.PreferredRoom,
                    ReservedClasses = data.ReservedClasses ?? new List<string>()
                };
            }

            user.SetPasswordHash(data.Password);
            return user;
        }
    }
   public class AuthenticationService
    {
        private readonly UserRepository _userRepository;
        public AuthenticationService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public (bool success, string message) RegisterClient(string username, string password,string fullname)
        { var client = new Client(username, password, fullname);
            return _userRepository.AddUser(client);
        }
        public (bool success, string message) RegisterAdmin(string username, string password, string email, string fullname, string phone = "", string accessLevel = "standard")
        {
            var admin = new Admin(username, password, fullname, accessLevel);
            return _userRepository.AddUser(admin);
        }
        public (bool success, User user, string message) Login(string username, string password)
        {
            var user = _userRepository.GetUser(username);

            if (user == null)
                return (false, null, "Username sau parolă incorectă!");

            if (!user.VerifyPassword(password))
                return (false, null, "Username sau parolă incorectă!");

            return (true, user, $"Bun venit, {user.FullName}!");
        }
    }
    public class GymManagementSystem
    {
        private readonly FileManager _fileManager;
        private readonly UserRepository _userRepository;
        private readonly AuthenticationService _authService;
        private User _currentUser;
        public Dictionary<string, FitnessRoom> Rooms { get; set; }

        public GymManagementSystem(string usersFile = "users.json")
        {
            _fileManager = new FileManager(usersFile);
            _userRepository = new UserRepository(_fileManager);
            _authService = new AuthenticationService(_userRepository);
            _currentUser = null;
            Rooms = new Dictionary<string, FitnessRoom>();
            InitializeRooms();
            LoadClassesFromFile();
        }
        private void LoadClassesFromFile()
        {
            var gymData = _fileManager.LoadGymData();
        
            foreach (var kvp in gymData.RoomClasses)
            {
                string roomKey = kvp.Key;
                if (Rooms.ContainsKey(roomKey))
                {
                    foreach (var classData in kvp.Value)
                    {
                        var trainer = new Trainer(classData.TrainerName, classData.TrainerSpecialization);
                        var classType = (ClassType)Enum.Parse(typeof(ClassType), classData.Type);
                    
                        var classSession = new ClassSession(
                            classType, 
                            trainer, 
                            classData.DurationMinutes, 
                            classData.MaxCapacity, 
                            classData.StartTime, 
                            classData.RoomLocation
                        );
                    
                        classSession.Id = classData.Id;
                        foreach (var participant in classData.Participants)
                        {
                            classSession.Participants.Add(participant);
                        }
                    
                        Rooms[roomKey].Classes.Add(classSession);
                    }
                }
            }
        }
        private void SaveClassesToFile()
        {
            var gymData = _fileManager.LoadGymData();
            gymData.RoomClasses = new Dictionary<string, List<ClassSessionData>>();
        
            foreach (var kvp in Rooms)
            {
                string roomKey = kvp.Key;
                var classesList = new List<ClassSessionData>();
            
                foreach (var classSession in kvp.Value.Classes)
                {
                    classesList.Add(new ClassSessionData
                    {
                        Id = classSession.Id,
                        Type = classSession.Type.ToString(),
                        TrainerName = classSession.Trainer.Name,
                        TrainerSpecialization = classSession.Trainer.Specialization,
                        DurationMinutes = classSession.DurationMinutes,
                        MaxCapacity = classSession.MaxCapacity,
                        Participants = classSession.Participants,
                        StartTime = classSession.StartTime,
                        RoomLocation = classSession.RoomLocation
                    });
                }
            
                gymData.RoomClasses[roomKey] = classesList;
            }
        
            _fileManager.SaveGymData(gymData);
        }

        private void InitializeRooms()
        {
            Rooms.Add("Sala1", new FitnessRoom("Sala Fitness Central", "Str. Victoriei nr. 10"));
            Rooms.Add("Sala2", new FitnessRoom("Sala Fitness Nord", "Bd. Aviatorilor nr. 25"));
            Rooms.Add("Sala3", new FitnessRoom("Sala Fitness Sud", "Calea Văcărești nr. 150"));
        }
        public (bool success, string message) RegisterClient(string username, string password, string fullname)
        {
            return _authService.RegisterClient(username, password, fullname);
        }
        public (bool success, string message) RegisterAdmin(string username, string password, string fullname, string accessLevel = "standard")
        {
            return _authService.RegisterAdmin(username, password, fullname, accessLevel);
        }

        public (bool success, string message) Login(string username, string password)
        {
            var (success, user, message) = _authService.Login(username, password);
            if (success)
                _currentUser = user;
            return (success, message);
        }
        public string Logout()
        {
            _currentUser = null;
            return "Deconectat cu succes!";
        }
        public Dictionary<string, object> GetCurrentUserInfo()
        {
            return _currentUser?.GetData();
        }
        public void DisplaySubscriptionOptions()
        {
            var lunar = new Subscription(SubscriptionType.Lunar);
            var anual = new Subscription(SubscriptionType.Anual);
            
            lunar.DisplayInfo();
            anual.DisplayInfo();
        }
        public string ActivateMembership(SubscriptionType type)
        {
            if (_currentUser is Client client)
            {
                string result = client.ActivateMembership(type);
                _userRepository.UpdateUser(client);
                return result;
            }
            return "Doar clienții pot activa abonamente!";
        }
        public string AddVisit()
        {
            if (_currentUser is Client client)
            {
                string result = client.AddVisit();
                _userRepository.UpdateUser(client);
                return result;
            }
            return "Doar clienții pot înregistra vizite!";
        }
        public void DisplayAllRooms()
        {
            Console.WriteLine("SĂLI DE FITNESS DISPONIBILE");
            foreach (var room in Rooms.Values)
            {
                room.DisplayInfo();
            }
        }
        public string CheckInToZone(string roomKey, string zoneName)
        {
            if (_currentUser is Client client)
            {
                if (!Rooms.ContainsKey(roomKey))
                    return "Sala nu există!";

                string result = client.CheckInToZone(Rooms[roomKey], zoneName);
                _userRepository.UpdateUser(client);
                return result;
            }
            return "Doar clienții pot face check-in!";
        }
        public string CheckOutFromZone(string roomKey)
        {
            if (_currentUser is Client client)
            {
                if (!Rooms.ContainsKey(roomKey))
                    return "Sala nu există!";

                string result = client.CheckOutFromZone(Rooms[roomKey]);
                _userRepository.UpdateUser(client);
                return result;
            }
            return "Doar clienții pot face check-out!";
        }
        public string AddClass(string roomKey, ClassType type, string trainerName, string trainerSpec, 
                               string trainerEmail, string trainerPhone, int duration, int maxCapacity, DateTime startTime)
        {
            if (!(_currentUser is Admin))
                return "Doar administratorii pot adăuga clase!";

            if (!Rooms.ContainsKey(roomKey))
                return "Sala nu există!";

            var trainer = new Trainer(trainerName, trainerSpec);
            var classSession = new ClassSession(type, trainer, duration, maxCapacity, startTime, Rooms[roomKey].Name);
            Rooms[roomKey].AddClass(classSession);
            SaveClassesToFile();
            return $"Clasa {type} adăugată cu succes! ID: {classSession.Id}";
        }
        public string RemoveClass(string roomKey, string classId)
        {
            if (!(_currentUser is Admin))
                return "Doar administratorii pot șterge clase!";

            if (!Rooms.ContainsKey(roomKey))
                return "Sala nu există!";

            if (Rooms[roomKey].RemoveClass(classId))
            {
                SaveClassesToFile();
                return "Clasa ștearsă cu succes!";
            }

            return "Clasa nu a fost găsită!";
        }
        public void DisplayAdminStatistics()
        {
            if (!(_currentUser is Admin))
            {
                Console.WriteLine("Doar administratorii pot vizualiza statisticile!");
                return;
            }
            var allUsers = _userRepository.GetAllUsers();
            var clients = allUsers.Values.Where(u => u.UserType == "Client").ToList();
            
            int activeSubscriptions = clients.Count(c => c.MembershipStatus == "activ");
            int totalClients = clients.Count;
            
            Console.WriteLine("STATISTICI ADMINISTRATOR");
            Console.WriteLine($"\n ABONAMENTE:");
            Console.WriteLine($"   Abonamente active: {activeSubscriptions}");
            Console.WriteLine($"   Total clienți: {totalClients}");
            Console.WriteLine($"   Rată activare: {(totalClients > 0 ? (double)activeSubscriptions / totalClients * 100 : 0):F1}%");

            Console.WriteLine($"\n️ GRAD DE OCUPARE ZONE:");
            foreach (var room in Rooms.Values)
            {
                Console.WriteLine($"\n   {room.Name}:");
                foreach (var zone in room.Zones.Values)
                {
                    Console.WriteLine($"      {zone.Name}: {zone.CurrentOccupancy}/{zone.MaxCapacity} ({zone.GetOccupancyRate():F1}%)");
                }
            }
            Console.WriteLine($"\n CLASE PROGRAMATE:");
            int totalClasses = 0;
            int totalParticipants = 0;
            foreach (var room in Rooms.Values)
            {
                totalClasses += room.Classes.Count;
                totalParticipants += room.Classes.Sum(c => c.Participants.Count);
            }
            Console.WriteLine($"   Total clase: {totalClasses}");
            Console.WriteLine($"   Total participanți înscriși: {totalParticipants}");
        }
        public void DisplayAllClasses()
        {
            Console.WriteLine("TOATE CLASELE DISPONIBILE");
            bool hasClasses = false;
            foreach (var room in Rooms.Values)
            {
                if (room.Classes.Count > 0)
                {
                    hasClasses = true;
                    room.DisplayClasses();
                }
            }

            if (!hasClasses)
            {
                Console.WriteLine("\n  Nu există clase programate momentan.");
            }
        }
        public string ReserveClass(string roomKey, string classId)
        {
            if (!(_currentUser is Client client))
                return "Doar clienții pot rezerva clase!";

            if (client.MembershipStatus != "activ")
                return "Trebuie să ai un abonament activ pentru a rezerva clase!";

            if (!Rooms.ContainsKey(roomKey))
                return "Sala nu există!";

            var classSession = Rooms[roomKey].GetClass(classId);
            if (classSession == null)
                return "Clasa nu există!";

            if (classSession.Reserve(client.Username))
            {
                client.ReservedClasses.Add(classId);
                _userRepository.UpdateUser(client);
                SaveClassesToFile();
                return $"Rezervare confirmată pentru clasa {classSession.Type} pe {classSession.StartTime:yyyy-MM-dd HH:mm}!";
            }

            return "Nu s-a putut face rezervarea. Clasa este complet sau ești deja înscris!";
        }
        public string CancelReservation(string roomKey, string classId)
        {
            if (!(_currentUser is Client client))
                return "Doar clienții pot anula rezervări!";

            if (!Rooms.ContainsKey(roomKey))
                return "Sala nu există!";

            var classSession = Rooms[roomKey].GetClass(classId);
            if (classSession == null)
                return "Clasa nu există!";

            if (classSession.CancelReservation(client.Username))
            {
                client.ReservedClasses.Remove(classId);
                _userRepository.UpdateUser(client);
                SaveClassesToFile();
                return "Rezervare anulată cu succes!";
            }

            return "Nu ai o rezervare pentru această clasă!";
        }
        public string CancelMembership()
        {
            if (_currentUser is Client client)
            {
                string result = client.CancelMembership();
                var (success, message) = _userRepository.UpdateUser(client);
        
                if (!success)
                {
                    return $"Eroare la salvare: {message}";
                }
                _currentUser = _userRepository.GetUser(client.Username);
        
                return result;
            }
            return "Doar clienții pot anula abonamente!";
        }
        public void DisplayMyReservations()
        {
            if (!(_currentUser is Client client))
            {
                Console.WriteLine("Doar clienții pot vedea rezervările!");
                return;
            }
            
            Console.WriteLine("REZERVĂRILE MELE");
            if (client.ReservedClasses.Count == 0)
            {
                Console.WriteLine("\n  Nu ai nicio rezervare.");
                return;
            }

            bool found = false;
            foreach (var room in Rooms.Values)
            {
                foreach (var classId in client.ReservedClasses)
                {
                    var classSession = room.GetClass(classId);
                    if (classSession != null)
                    {
                        found = true;
                        classSession.DisplayInfo();
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("\n  Clasele rezervate nu mai sunt disponibile.");
            }
        }
    } 
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string usersFile = Path.Combine(desktopPath, "users.json");
            Console.WriteLine($"Fișierul va fi salvat în: {usersFile}\n");
            var gym = new GymManagementSystem(usersFile);
            Console.WriteLine("GYM MANAGEMENT");

            while (true)
            {
                Console.WriteLine("MENIU PRINCIPAL");
                Console.WriteLine(" 1. Înregistrare Client              ");
                Console.WriteLine(" 2. Înregistrare Admin               ");
                Console.WriteLine(" 3. Login                            ");
                Console.WriteLine(" 4. Info utilizator curent           ");
                Console.WriteLine(" 5. Vizualizare tipuri abonamente    ");
                Console.WriteLine(" 6. Activare abonament               ");
                Console.WriteLine(" 6a. Renunțare la abonament          "); 
                Console.WriteLine(" 7. Vizualizare săli fitness         ");
                Console.WriteLine(" 8. Check-in în zonă                 ");
                Console.WriteLine(" 9. Check-out din zonă               ");
                Console.WriteLine(" 10. Înregistrare vizită             ");
                Console.WriteLine(" --- CLASE ---                       ");
                Console.WriteLine(" 11. Vizualizare toate clasele       ");
                Console.WriteLine(" 12. Rezervare clasă (Client)        ");
                Console.WriteLine(" 13. Anulare rezervare (Client)      ");
                Console.WriteLine(" 14. Rezervările mele (Client)       ");
                Console.WriteLine(" --- ADMIN ---                       ");
                Console.WriteLine(" 15. Adăugare clasă (Admin)          ");
                Console.WriteLine(" 16. Ștergere clasă (Admin)          ");
                Console.WriteLine(" 17. Statistici (Admin)              ");
                Console.WriteLine(" --- GENERAL ---                     ");
                Console.WriteLine(" 18. Logout                          ");
                Console.WriteLine(" 19. Ieșire                          ");
                Console.Write("\nAlege opțiunea: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\n--- ÎNREGISTRARE CLIENT ---");
                        Console.Write("Username: ");
                        string username = Console.ReadLine();
                        Console.Write("Parolă: ");
                        string password = Console.ReadLine();
                        Console.Write("Nume complet: ");
                        string fullName = Console.ReadLine();

                        var (success1, message1) = gym.RegisterClient(username, password, fullName);
                        Console.WriteLine($"\n{message1}");
                        break;

                    case "2":
                        Console.WriteLine("\n--- ÎNREGISTRARE ADMIN ---");
                        Console.Write("Username: ");
                        string adminUser = Console.ReadLine();
                        Console.Write("Parolă: ");
                        string adminPass = Console.ReadLine();
                        Console.Write("Nume complet: ");
                        string adminName = Console.ReadLine();

                        var (success2, message2) = gym.RegisterAdmin(adminUser, adminPass, adminName);
                        Console.WriteLine($"\n{message2}");
                        break;

                    case "3":
                        Console.WriteLine("\n--- LOGIN ---");
                        Console.Write("Username: ");
                        string loginUsername = Console.ReadLine();
                        Console.Write("Parolă: ");
                        string loginPassword = Console.ReadLine();

                        var (success3, message3) = gym.Login(loginUsername, loginPassword);
                        Console.WriteLine($"\n{message3}");
                        break;

                    case "4":
                        var info = gym.GetCurrentUserInfo();
                        if (info != null)
                        {
                            Console.WriteLine("\n--- INFORMAȚII UTILIZATOR ---");
                            foreach (var kvp in info)
                            {
                                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nNu ești autentificat!");
                        }
                        break;

                    case "5":
                        gym.DisplaySubscriptionOptions();
                        break;

                    case "6":
                        Console.WriteLine("\n--- ACTIVARE ABONAMENT ---");
                        Console.WriteLine("1. Lunar (150 RON/lună)");
                        Console.WriteLine("2. Anual (1500 RON/an)");
                        Console.Write("Alege tipul abonamentului: ");
                        string subChoice = Console.ReadLine();

                        SubscriptionType type = subChoice == "1" ? SubscriptionType.Lunar : SubscriptionType.Anual;
                        string result6 = gym.ActivateMembership(type);
                        Console.WriteLine($"\n{result6}");
                        break;
                    
                    case "6a":
                        Console.WriteLine("\n--- RENUNȚARE LA ABONAMENT ---");
                        Console.WriteLine("ATENȚIE: Această acțiune va anula abonamentul tău curent!");
                        Console.Write("Confirmă (da/nu): ");
                        string confirm = Console.ReadLine();
    
                        if (confirm.ToLower() == "da")
                        {
                            string resultCancel = gym.CancelMembership();
                            Console.WriteLine($"\n{resultCancel}");
                        }
                        else
                        {
                            Console.WriteLine("\nOperațiune anulată.");
                        }
                        break;

                    case "7":
                        gym.DisplayAllRooms();
                        break;

                    case "8":
                        Console.WriteLine("\n--- CHECK-IN ÎN ZONĂ ---");
                        Console.WriteLine("Săli: Sala1 | Sala2 | Sala3");
                        Console.Write("Alege sala: ");
                        string roomKey8 = Console.ReadLine();
                        Console.WriteLine("Zone: Cardio | Forta | Tractiuni");
                        Console.Write("Alege zona: ");
                        string zone8 = Console.ReadLine();

                        string result8 = gym.CheckInToZone(roomKey8, zone8);
                        Console.WriteLine($"\n{result8}");
                        break;

                    case "9":
                        Console.WriteLine("\n--- CHECK-OUT DIN ZONĂ ---");
                        Console.Write("Sala (Sala1/Sala2/Sala3): ");
                        string roomKey9 = Console.ReadLine();

                        string result9 = gym.CheckOutFromZone(roomKey9);
                        Console.WriteLine($"\n{result9}");
                        break;

                    case "10":
                        string result10 = gym.AddVisit();
                        Console.WriteLine($"\n{result10}");
                        break;

                    case "11":
                        gym.DisplayAllClasses();
                        break;

                    case "12":
                        Console.WriteLine("\n--- REZERVARE CLASĂ ---");
                        gym.DisplayAllClasses();
                        Console.Write("\nSala (Sala1/Sala2/Sala3): ");
                        string roomKey12 = Console.ReadLine();
                        Console.Write("ID Clasă: ");
                        string classId12 = Console.ReadLine();

                        string result12 = gym.ReserveClass(roomKey12, classId12);
                        Console.WriteLine($"\n{result12}");
                        break;

                    case "13":
                        Console.WriteLine("\n--- ANULARE REZERVARE ---");
                        gym.DisplayMyReservations();
                        Console.Write("\nSala (Sala1/Sala2/Sala3): ");
                        string roomKey13 = Console.ReadLine();
                        Console.Write("ID Clasă: ");
                        string classId13 = Console.ReadLine();

                        string result13 = gym.CancelReservation(roomKey13, classId13);
                        Console.WriteLine($"\n{result13}");
                        break;

                    case "14":
                        gym.DisplayMyReservations();
                        break;

                    case "15":
                        Console.WriteLine("\n--- ADĂUGARE CLASĂ (ADMIN) ---");
                        Console.Write("Sala (Sala1/Sala2/Sala3): ");
                        string roomKey15 = Console.ReadLine();
                        Console.WriteLine("Tip clasă: 1.Fitness | 2.Pilates | 3.Yoga");
                        Console.Write("Alege: ");
                        string typeChoice = Console.ReadLine();
                        ClassType classType = typeChoice == "1" ? ClassType.Fitness : 
                                             typeChoice == "2" ? ClassType.Pilates : ClassType.Yoga;
                        
                        Console.Write("Nume antrenor: ");
                        string trainerName = Console.ReadLine();
                        Console.Write("Specializare antrenor: ");
                        string trainerSpec = Console.ReadLine();
                        Console.Write("Email antrenor: ");
                        string trainerEmail = Console.ReadLine();
                        Console.Write("Telefon antrenor: ");
                        string trainerPhone = Console.ReadLine();
                        Console.Write("Durată (minute): ");
                        int duration = int.Parse(Console.ReadLine());
                        Console.Write("Capacitate maximă: ");
                        int maxCap = int.Parse(Console.ReadLine());
                        Console.Write("Data și ora (yyyy-MM-dd HH:mm): ");
                        DateTime startTime = DateTime.Parse(Console.ReadLine());

                        string result15 = gym.AddClass(roomKey15, classType, trainerName, trainerSpec, 
                                                       trainerEmail, trainerPhone, duration, maxCap, startTime);
                        Console.WriteLine($"\n{result15}");
                        break;

                    case "16":
                        Console.WriteLine("\n--- ȘTERGERE CLASĂ (ADMIN) ---");
                        Console.Write("Sala (Sala1/Sala2/Sala3): ");
                        string roomKey16 = Console.ReadLine();
                        Console.Write("ID Clasă: ");
                        string classId16 = Console.ReadLine();

                        string result16 = gym.RemoveClass(roomKey16, classId16);
                        Console.WriteLine($"\n{result16}");
                        break;

                    case "17":
                        gym.DisplayAdminStatistics();
                        break;

                    case "18":
                        string result18 = gym.Logout();
                        Console.WriteLine($"\n{result18}");
                        break;

                    case "19":
                        Console.WriteLine("\nLa revedere!");
                        return;

                    default:
                        Console.WriteLine("\nOpțiune invalidă!");
                        break;
                }
            }
        }
    }   
}