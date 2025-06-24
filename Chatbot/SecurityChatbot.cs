using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Chatbot
{
    public class SecurityChatbot : ChatbotBase, IResponder
    {
        private bool _running;
        private Dictionary<string, string[]> _responses;
        private Dictionary<string, string[]> _tips;
        private Dictionary<string, List<string>> _keywords;
        private List<string> _discussedTopics;
        private string _currentEmotion = "neutral";
        private int _emotionIntensity = 1;
        private List<string> _emotionalHistory = new List<string>();
        private Dictionary<string, string> _userInterests = new Dictionary<string, string>();

        public SecurityChatbot(string username, string audioPath) : base(username, audioPath)
        {
            _running = true;
            _discussedTopics = new List<string>();
            _responses = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _tips = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _keywords = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            InitializeResponses();
            InitializeTips();
            InitializeKeywords();
        }

        public SecurityChatbot(string audioPath) : this("User", audioPath) { }

        public List<string> GetAvailableTopics()
        {
            return _responses.Keys.OrderBy(t => t).ToList();
        }

        protected override string DetectSentiment(string input)
        {
            var sentiment = base.DetectSentiment(input);

            if (_currentEmotion == sentiment)
            {
                _emotionIntensity = Math.Min(_emotionIntensity + 1, 3);
            }
            else
            {
                _currentEmotion = sentiment;
                _emotionIntensity = 1;
            }

            _emotionalHistory.Add(sentiment);
            return sentiment;
        }

        protected override string GetSentimentResponse(string sentiment, string topic = null)
        {
            if (topic != null && (sentiment == "happy" || sentiment == "excited"))
            {
                string interestNote = _emotionalHistory.Last();
                if (!_userInterests.ContainsKey(topic))
                {
                    _userInterests[topic] = interestNote;
                }
            }

            if (sentiment != "neutral" && topic == null)
            {
                var probingQuestions = new Dictionary<string, string>
                {
                    ["worried"] = $"I've noticed you've seemed concerned. {(_emotionalHistory.Count(x => x == "worried") > 1 ? "Still worried about cybersecurity risks?" : "What security aspects concern you most?")}",
                    ["angry"] = $"I hear frustration in your words. {(_emotionalHistory.Count(x => x == "angry") > 1 ? "This still bothers you, doesn't it?" : "What security issue is upsetting you?")}",
                    ["sad"] = $"You seem upset. {(_emotionalHistory.Count(x => x == "sad") > 1 ? "This still makes you sad?" : "Would sharing what's bothering you help?")}",
                    ["happy"] = $"You sound positive! {(_emotionalHistory.Count(x => x == "happy") > 1 ? "Still excited about cybersecurity?" : "What security topics interest you?")}",
                    ["confused"] = $"I sense confusion. {(_emotionalHistory.Count(x => x == "confused") > 1 ? "Still unclear about this?" : "What concepts would you like explained?")}"
                };

                return probingQuestions.ContainsKey(sentiment)
                    ? probingQuestions[sentiment]
                    : "How can I help you with cybersecurity today?";
            }

            var intensityPhrases = new Dictionary<string, string[]>
            {
                ["worried"] = new[] { "I understand your concern", "This is clearly worrying you", "I can see this really troubles you" },
                ["angry"] = new[] { "I hear your frustration", "You're clearly upset about this", "This has made you quite angry" },
                ["sad"] = new[] { "I sense your unease", "This seems to bother you deeply", "You seem really affected by this" },
                ["happy"] = new[] { "Your enthusiasm is great", "I love your positive attitude", "Your excitement is contagious" },
                ["confused"] = new[] { "This can be confusing", "I understand your confusion", "This is quite perplexing" }
            };

            var response = base.GetSentimentResponse(sentiment, topic);

            if (intensityPhrases.ContainsKey(sentiment) && _emotionIntensity > 1)
            {
                response = intensityPhrases[sentiment][Math.Min(_emotionIntensity - 1, 2)] + ". " + response;
            }

            return response;
        }

        private void InitializeResponses()
        {
            _responses = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "cybersecurity", new[] {
                    "🛡️ Cybersecurity protects systems, networks, and data from digital attacks through technologies and best practices.",
                    "🌐 Cybersecurity spans technical defenses, user education, and organizational policies to create layered protection."
                }},
                { "information security", new[] {
                    "🔐 InfoSec focuses on protecting data confidentiality, integrity, and availability (CIA triad) throughout its lifecycle.",
                    "📊 Information security manages risks to sensitive data through controls like encryption and access management."
                }},
                { "hackers", new[] {
                    "👨‍💻 Hackers range from ethical 'white hats' to criminal 'black hats' and activist 'hacktivists' with different motives.",
                    "🕵️‍♂️ Advanced Persistent Threats (APTs) are sophisticated hackers often backed by nation-states for espionage."
                }},
                { "script kiddies", new[] {
                    "👶 Script kiddies use pre-made tools without technical knowledge - often causing damage through inexperience.",
                    "⚠️ While not highly skilled, script kiddies can still deploy damaging attacks using online tools."
                }},
                { "phishing", new[] {
                    "🎣 Phishing uses fake communications to steal data. Variants include spear phishing (targeted) and whaling (executive targets).",
                    "📧 Smishing (SMS) and vishing (voice) are phone-based phishing methods becoming more common."
                }},
                { "malware", new[] {
                    "🦠 Malware includes viruses, worms, trojans, spyware - each with different infection methods and payloads.",
                    "💣 Logic bombs are malware that activates when specific conditions are met, often by disgruntled insiders."
                }},
                { "trojan", new[] {
                    "🐴 Trojans disguise themselves as legitimate software while creating backdoors for attackers.",
                    "🎁 Unlike viruses, Trojans don't self-replicate but are equally dangerous for data theft."
                }},
                { "ransomware", new[] {
                    "💰 Ransomware encrypts files until payment is made. New versions now also steal data (double extortion).",
                    "⏳ The average ransomware payment increased 300% in 2023, making prevention critical."
                }},
                { "social engineering", new[] {
                    "🎭 Social engineering manipulates human psychology rather than technical vulnerabilities.",
                    "👔 Business Email Compromise (BEC) scams use impersonation to trick employees into wiring money."
                }},
                { "pretexting", new[] {
                    "📖 Pretexting creates fabricated scenarios to obtain information (e.g., pretending to be IT support).",
                    "📞 Vishing (voice phishing) often uses pretexting to gain trust over the phone."
                }},
                { "identity theft", new[] {
                    "🆔 Identity theft uses personal info to commit fraud. Dark web markets sell stolen identities for as little as $10.",
                    "💳 Synthetic identity theft combines real and fake information to create new fraudulent identities."
                }},
                { "credit card fraud", new[] {
                    "💳 Card skimmers on ATMs and e-skimming malware on websites steal payment data in different ways.",
                    "🛒 Card-not-present (CNP) fraud increased 40% during pandemic as online shopping grew."
                }},
                { "firewall", new[] {
                    "🧱 Next-gen firewalls (NGFW) add intrusion prevention, app awareness, and cloud integration to traditional filtering.",
                    "☁️ Cloud firewalls protect cloud infrastructure and can scale dynamically with traffic loads."
                }},
                { "antivirus", new[] {
                    "🛡️ Modern EDR (Endpoint Detection & Response) solutions go beyond signature detection to behavioral analysis.",
                    "🔍 Sandboxing isolates suspicious files in virtual environments to analyze behavior safely."
                }},
                { "vpn", new[] {
                    "🔒 Zero Trust VPNs verify each request as if originating from an open network, reducing trust assumptions.",
                    "🌍 VPN protocols like WireGuard offer faster speeds while maintaining strong encryption."
                }},
                { "zero trust", new[] {
                    "❌ Zero Trust architecture assumes breach and verifies each request - 'never trust, always verify'.",
                    "🔄 Continuous authentication in Zero Trust models checks user/device status throughout sessions."
                }},
                { "ai security", new[] {
                    "🤖 AI-powered attacks can automate phishing, bypass CAPTCHAs, and create deepfake voice scams.",
                    "🛡️ Defensive AI detects anomalies and responds to threats faster than human teams alone."
                }},
                { "iot threats", new[] {
                    "📡 Default credentials and lack of updates make IoT devices prime targets for botnet recruitment.",
                    "🏠 Smart home devices often have minimal security, potentially exposing home networks."
                }},
                { "privacy", new[] {
                    "👁️ Privacy focuses on controlling personal data collection and usage, distinct from security.",
                    "🌐 GDPR, CCPA and other regulations enforce privacy rights with strict compliance requirements."
                }},
                { "encryption", new[] {
                    "🔐 End-to-end encryption ensures only communicating users can read messages - not even service providers.",
                    "💾 Full disk encryption protects data at rest if devices are lost or stolen."
                }},
                { "password", new[] {
                    "🔑 NIST now recommends longer passphrases over complex short passwords changed frequently.",
                    "🧠 Password managers generate/store strong credentials and only require remembering one master password."
                }},
                { "two factor", new[] {
                    "📲 2FA methods include SMS codes, authenticator apps, hardware tokens, and biometric verification.",
                    "⚠️ SMS-based 2FA is vulnerable to SIM swapping attacks - use app-based when possible."
                }},
                { "data breach", new[] {
                    "📉 The average cost of a data breach reached $4.45 million in 2023 according to IBM research.",
                    "⏱️ Breach containment under 30 days saves over $1 million compared to longer response times."
                }},
                { "disaster recovery", new[] {
                    "🔥 3-2-1 backup rule: Keep 3 copies, on 2 different media, with 1 offsite/cloud copy.",
                    "🔄 Regular disaster recovery testing ensures backups actually work when needed."
                }},
                { "scam", new[] {
                    "🕵️ Scams come in many forms - phishing emails, fake tech support calls, romance scams, and more.",
                    "💡 Always verify requests for money or information through a separate communication channel."
                }}
            };
        }

        private void InitializeTips()
        {
            _tips = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", new[] {
                    "💡 Tip: Check email headers for mismatched sender addresses in suspicious emails.",
                    "💡 Tip: Never provide credentials via links in messages - always go directly to the official site.",
                    "💡 Tip: Enable 'Enhanced Protection' in Gmail or similar filters in other email clients."
                }},
                { "malware", new[] {
                    "💡 Tip: Disable macros in Office documents from unknown senders to prevent macro malware.",
                    "💡 Tip: Regularly review browser extensions and remove unused ones to reduce attack surface.",
                    "💡 Tip: Use a standard user account rather than administrator for daily computing."
                }},
                { "password", new[] {
                    "💡 Tip: Create memorable passphrases like 'CorrectHorseBatteryStaple' instead of complex passwords.",
                    "💡 Tip: Check haveibeenpwned.com to see if your credentials appear in known breaches.",
                    "💡 Tip: Use different password managers for personal and work credentials."
                }},
                { "vpn", new[] {
                    "💡 Tip: Always connect to VPN before using public WiFi in airports, hotels, or cafes.",
                    "💡 Tip: Choose VPN providers with independent security audits and no-log policies.",
                    "💡 Tip: Test for DNS leaks after connecting to ensure all traffic routes through VPN."
                }},
                { "social engineering", new[] {
                    "💡 Tip: Verify unusual requests through a separate communication channel (call back known number).",
                    "💡 Tip: Be wary of urgent requests - scammers create false emergencies to bypass scrutiny.",
                    "💡 Tip: Never confirm sensitive information to callers who initiate contact with you."
                }},
                { "ransomware", new[] {
                    "💡 Tip: Maintain air-gapped backups that malware can't reach to delete or encrypt.",
                    "💡 Tip: Disable RDP (Remote Desktop Protocol) if not needed to prevent common attack vectors.",
                    "💡 Tip: Enable 'Controlled Folder Access' in Windows Defender to protect key directories."
                }},
                { "privacy", new[] {
                    "💡 Tip: Review app permissions regularly and revoke unnecessary access to camera/microphone.",
                    "💡 Tip: Use privacy-focused browsers like Brave or Firefox with strict tracking protection.",
                    "💡 Tip: Enable 'Find My Device' features to remotely wipe lost phones/tablets."
                }},
                { "iot security", new[] {
                    "💡 Tip: Change default credentials on smart devices before connecting to your network.",
                    "💡 Tip: Place IoT devices on a separate network segment from computers/phones.",
                    "💡 Tip: Disable Universal Plug and Play (UPnP) which can expose devices to the internet."
                }},
                { "browsing", new[] {
                    "💡 Tip: Look for HTTPS and padlock icon before entering sensitive information on websites.",
                    "💡 Tip: Use browser sandboxing features like Chrome's 'Enhanced Protection' mode.",
                    "💡 Tip: Clear cookies regularly or use private browsing for sensitive activities."
                }},
                { "encryption", new[] {
                    "💡 Tip: Enable full-disk encryption on all devices (BitLocker/FileVault) in case of theft.",
                    "💡 Tip: Use Signal or other E2E encrypted apps for sensitive communications.",
                    "💡 Tip: Verify PGP key fingerprints out-of-band when using encrypted email."
                }}
            };
        }

        private void InitializeKeywords()
        {
            _keywords = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "cybersecurity", new List<string> {
                    "infosec", "digital security", "cyber protection", "online safety",
                    "network security", "data protection", "cyber defense", "internet security"
                }},
                { "information security", new List<string> {
                    "data security", "infosec", "c.i.a. triad", "security principles",
                    "confidentiality", "integrity", "availability", "security controls"
                }},
                { "hackers", new List<string> {
                    "black hat", "white hat", "gray hat", "cybercriminals",
                    "threat actors", "attackers", "bad actors", "script kiddies",
                    "hacktivists", "nation state", "cyber spies", "insider threats"
                }},
                { "script kiddies", new List<string> {
                    "skiddies", "amateur hackers", "beginner hackers", "copy-paste hackers",
                    "unskilled attackers", "tool users"
                }},
                { "phishing", new List<string> {
                    "spear phishing", "whaling", "smishing", "vishing", "email fraud",
                    "brand impersonation", "clone phishing", "angler phishing",
                    "deceptive emails", "credential harvesting", "login scams"
                }},
                { "malware", new List<string> {
                    "viruses", "worms", "trojans", "spyware", "adware", "rootkits",
                    "keyloggers", "botnets", "logic bombs", "fileless malware",
                    "polymorphic malware", "macro viruses", "crypto malware"
                }},
                { "ransomware", new List<string> {
                    "crypto ransomware", "locker ransomware", "doxware", "leakware",
                    "double extortion", "file encryption", "data hostage",
                    "ransom attacks", "decryption demand"
                }},
                { "social engineering", new List<string> {
                    "human hacking", "pretexting", "baiting", "quid pro quo",
                    "tailgating", "impersonation", "psychological manipulation",
                    "confidence tricks", "authority exploitation"
                }},
                { "identity theft", new List<string> {
                    "identity fraud", "personal data theft", "synthetic identity",
                    "credit fraud", "account takeover", "identity cloning",
                    "medical identity theft", "tax fraud", "benefit fraud"
                }},
                { "credit card fraud", new List<string> {
                    "card skimming", "e-skimming", "card-not-present fraud", "CNP fraud",
                    "card cloning", "BIN attacks", "carding", "payment fraud"
                }},
                { "firewall", new List<string> {
                    "network firewall", "host firewall", "web firewall", "WAF",
                    "next-gen firewall", "NGFW", "packet filtering", "stateful inspection",
                    "application firewall", "cloud firewall"
                }},
                { "antivirus", new List<string> {
                    "endpoint protection", "malware scanner", "virus protection",
                    "EDR", "endpoint detection", "XDR", "threat prevention",
                    "signature detection", "heuristic analysis"
                }},
                { "vpn", new List<string> {
                    "virtual private network", "secure tunnel", "encrypted connection",
                    "remote access", "IP masking", "geo-spoofing", "privacy tunnel",
                    "wireguard", "openvpn", "IPsec"
                }},
                { "zero trust", new List<string> {
                    "ZTNA", "never trust", "always verify", "microsegmentation",
                    "identity verification", "continuous authentication",
                    "least privilege access", "context-aware security"
                }},
                { "ai security", new List<string> {
                    "AI attacks", "machine learning security", "adversarial AI",
                    "deepfake attacks", "AI phishing", "automated hacking",
                    "AI defense", "security AI"
                }},
                { "iot threats", new List<string> {
                    "smart device risks", "iot botnets", "connected device security",
                    "mirai malware", "default credentials", "iot vulnerabilities",
                    "smart home risks", "industrial iot security"
                }},
                { "privacy", new List<string> {
                    "data privacy", "personal information", "PII", "GDPR",
                    "CCPA", "data minimization", "right to be forgotten",
                    "privacy laws", "tracking protection", "data collection"
                }},
                { "encryption", new List<string> {
                    "data encryption", "end-to-end", "E2EE", "cryptography",
                    "public key", "private key", "symmetric", "asymmetric",
                    "AES", "RSA", "SSL/TLS", "quantum encryption"
                }},
                { "password", new List<string> {
                    "passphrase", "credential hygiene", "password manager",
                    "authentication", "login security", "password strength",
                    "credential stuffing", "password reuse", "brute force"
                }},
                { "two factor", new List<string> {
                    "2FA", "MFA", "multi-factor", "authentication app",
                    "security key", "U2F", "one-time password", "OTP",
                    "biometric verification", "push notification"
                }},
                { "data breach", new List<string> {
                    "data leak", "security incident", "records exposed",
                    "breach notification", "compromised data", "database hack",
                    "breach response", "breach disclosure"
                }},
                { "disaster recovery", new List<string> {
                    "DRP", "business continuity", "backup strategy",
                    "recovery plan", "BCDR", "failover", "RTO", "RPO",
                    "backup testing", "system restoration"
                }},
                { "scam", new List<string> {
                    "scams", "scamming", "fraud", "swindle", "con", "scheme",
                    "hoax", "rip-off", "deception", "trickery", "fake", "fraudulent",
                    "cheat", "dupe", "bamboozle", "hoodwink", "sham", "racket"
                }}
            };
        }

        public override void Greet(MainWindow window)
        {
            try
            {
                if (!string.IsNullOrEmpty(AudioPath))
                {
                    SoundPlayer player = new SoundPlayer(AudioPath);
                    player.PlaySync();
                }
            }
            catch (Exception ex)
            {
                window.AppendToChat("⚠️ Error playing greeting: " + ex.Message, Brushes.Red);
            }

            ArtDisplay.ShowAsciiTitle(window);
            
        }

        public override void StartChat()
        {
            // Not used in WPF version - chat is handled through UI events
        }

        public void Respond(string input, MainWindow window)
        {
            if (string.IsNullOrEmpty(Username) || Username == "User")
            {
                Username = input;
                window.AppendToChat($"Nice to meet you, {Username}! How can I help you with cybersecurity today?", Brushes.Magenta);
                return;
            }

            if (IsGeneralConversation(input))
            {
                HandleGeneralConversation(input, window);
                return;
            }

            if (IsTopicsRequest(input) || IsScamRequest(input))
            {
                HandleTopicsAndScamsRequest(input, window);
                return;
            }

            if (IsFavoriteTopicInquiry(input))
            {
                HandleFavoriteTopicInquiry(window);
                return;
            }

            CurrentSentiment = DetectSentiment(input);
            string topic = FindTopicByInput(input);

            if (CurrentSentiment != "neutral")
            {
                window.AppendToChat(GetSentimentResponse(CurrentSentiment, topic), Brushes.Magenta);
            }

            if (input.Contains("tip") || input.Contains("advice") || input.Contains("suggestion"))
            {
                ProvideRandomTip(topic, window);
                return;
            }

            if (topic != null)
            {
                TrackTopicInterest(topic);
                LastTopic = topic;

                if (!_discussedTopics.Contains(topic))
                    _discussedTopics.Add(topic);

                window.AppendToChat(GetTopicResponse(topic), Brushes.Green);

                var favorite = GetFavoriteTopic();
                if (favorite != null && favorite == topic && TopicInterest[topic] == 3)
                {
                    window.AppendToChat($"\n✨ I notice you're really interested in {favorite}! Would you like to dive deeper into this topic?", Brushes.Cyan);
                }

                ProvideRandomTip(topic, window);
                return;
            }

            var currentFavorite = GetFavoriteTopic();
            if (currentFavorite != null && (input.Contains("favorite") || input.Contains("prefer")))
            {
                window.AppendToChat($"\n🔍 Based on our conversations, you seem most interested in {currentFavorite}.", Brushes.White);
                window.AppendToChat(GetTopicResponse(currentFavorite), Brushes.Green);
                ProvideRandomTip(currentFavorite, window);
                return;
            }

            ProvideFallbackResponse(input, window);
        }

        private bool IsTopicsRequest(string input)
        {
            string cleanInput = Regex.Replace(input.ToLower(), @"[^\w\s]", "");
            return Regex.IsMatch(cleanInput, @"\b(topics?|what (can|could) i ask|list (of )?topics?|available topics?|show me topics?)\b");
        }

        private bool IsScamRequest(string input)
        {
            string cleanInput = Regex.Replace(input.ToLower(), @"[^\w\s]", "");
            return Regex.IsMatch(cleanInput, @"\b(scams?|fraud|swindle|con|scheme|hoax|rip.?off|deception)\b");
        }

        private bool IsFavoriteTopicInquiry(string input)
        {
            string cleanInput = Regex.Replace(input.ToLower(), @"[^\w\s]", "");
            return Regex.IsMatch(cleanInput, @"\b(my (favorite|preferred|interested in) topic|what (am i|do you think) i like|what (am i|do i) (interested in|enjoy))\b");
        }

        private void HandleTopicsAndScamsRequest(string input, MainWindow window)
        {
            bool showTopics = IsTopicsRequest(input);
            bool showScams = IsScamRequest(input);

            if (showTopics)
            {
                window.AppendToChat("📚 Here are all the topics you can ask me about:", Brushes.White);
                window.AppendToChat(string.Join(", ", _responses.Keys.OrderBy(t => t)), Brushes.White);
            }

            if (showScams)
            {
                string scamResponse = GetTopicResponse("scam");
                window.AppendToChat("\n" + scamResponse, Brushes.Green);
                ProvideRandomTip("scam", window);
            }

            if (!showTopics && !showScams)
            {
                ProvideFallbackResponse(input, window);
            }
        }

        private void HandleFavoriteTopicInquiry(MainWindow window)
        {
            var favorite = GetFavoriteTopic();
            if (favorite != null)
            {
                window.AppendToChat($"🔍 Based on our conversations, you seem most interested in {favorite}!", Brushes.White);

                string response = GetTopicResponse(favorite);
                window.AppendToChat(response, Brushes.Green);

                if (_userInterests.ContainsKey(favorite))
                {
                    window.AppendToChat($"\n💭 Remember when you told me: \"{_userInterests[favorite]}\"", Brushes.White);
                }

                ProvideRandomTip(favorite, window);
            }
            else
            {
                window.AppendToChat("🤔 I haven't noticed a particular topic you're most interested in yet. " +
                                "Keep asking me questions and I'll learn your preferences!", Brushes.White);
            }
        }

        private bool IsGeneralConversation(string input)
        {
            string cleanInput = Regex.Replace(input.ToLower(), @"[^\w\s]", "");

            bool isHowAreYou = Regex.IsMatch(cleanInput, @"\b(how\s*are\s*you|hows\s*it\s*going|how\s*do\s*you\s*do)\b");
            bool isPurposeQuestion = Regex.IsMatch(cleanInput, @"\b(whats?|is|your)\s*(purpose|goal|function|job|role)\b");
            bool isTopicsQuestion = Regex.IsMatch(cleanInput, @"\b(what\s*(can|could)\s*i\s*ask|available\s*topic|list\s*topic|show\s*me\s*topic|tell\s*me\s*about\s*topic|topic)\b");
            bool isNameQuestion = Regex.IsMatch(cleanInput, @"\b(your\s*name|who\s*are\s*you|what\s*are\s*you\s*called)\b");
            bool isMyNameQuestion = Regex.IsMatch(cleanInput, @"\b(my\s*name|who\s*am\s*i|whats?(\s*my|\s*is\s*my)\s*name)\b");

            return isHowAreYou || isPurposeQuestion || isTopicsQuestion || isNameQuestion || isMyNameQuestion;
        }

        private void HandleGeneralConversation(string input, MainWindow window)
        {
            string cleanInput = Regex.Replace(input.ToLower(), @"[^\w\s]", "");

            if (Regex.IsMatch(cleanInput, @"\b(how\s*are\s*you|hows\s*it\s*going|how\s*do\s*you\s*do)\b"))
            {
                window.AppendToChat($"🤖 I'm functioning well today, thank you for asking {Username}! As a cybersecurity bot, I don't have feelings, but I'm ready to help you with any security questions.", Brushes.White);
            }
            else if (Regex.IsMatch(cleanInput, @"\b(whats?|is|your)\s*(purpose|goal|function|job|role)\b"))
            {
                window.AppendToChat($"🔐 My purpose is to help {Username} learn about cybersecurity in a friendly way. I can explain security concepts, give protection tips, and help you stay safe online.", Brushes.White);
            }
            else if (Regex.IsMatch(cleanInput, @"\b(what\s*(can|could)\s*i\s*ask|available\s*topic|list\s*topic|show\s*me\s*topic|tell\s*me\s*about\s*topic|topic)\b"))
            {
                window.AppendToChat("📚 Here are all the topics you can ask me about:", Brushes.White);
                window.AppendToChat(string.Join(", ", _responses.Keys.OrderBy(t => t)), Brushes.White);
                window.AppendToChat("\n💡 You can ask about any of these, or request 'tips' about specific topics!", Brushes.White);
            }
            else if (Regex.IsMatch(cleanInput, @"\b(your\s*name|who\s*are\s*you|what\s*are\s*you\s*called)\b"))
            {
                window.AppendToChat("🤖 I'm your Cybersecurity Awareness Bot, but you can call me whatever you like!", Brushes.White);
            }
            else if (Regex.IsMatch(cleanInput, @"\b(my\s*name|who\s*am\s*i|whats?(\s*my|\s*is\s*my)\s*name)\b"))
            {
                window.AppendToChat($"🪪 I know you as {Username}! If you'd like me to call you something else, just let me know.", Brushes.White);
            }
            else
            {
                ProvideFallbackResponse(input, window);
            }
        }

        private string FindTopicByInput(string input)
        {
            foreach (var topic in _responses.Keys)
            {
                if (input.IndexOf(topic, StringComparison.OrdinalIgnoreCase) >= 0)
                    return topic;
            }

            foreach (var kvp in _keywords)
            {
                foreach (var keyword in kvp.Value)
                {
                    if (Regex.IsMatch(input, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
                        return kvp.Key;
                }
            }

            return null;
        }


        private string GetTopicResponse(string topic)
        {
            if (_responses.ContainsKey(topic))
            {
                var responses = _responses[topic];

                int index = 0;
                if (CurrentSentiment != "neutral")
                {
                    index = 1 % responses.Length;

                    string sentimentIntro;
                    switch (CurrentSentiment)
                    {
                        case "happy":
                            sentimentIntro = "I'm glad you're excited about ";
                            break;
                        case "sad":
                            sentimentIntro = "I understand your concerns about ";
                            break;
                        case "angry":
                            sentimentIntro = "I hear your frustration with ";
                            break;
                        case "worried":
                            sentimentIntro = "I know can be worrying, but ";
                            break;
                        case "confused":
                            sentimentIntro = "Let me clarify ";
                            break;
                        default:
                            sentimentIntro = "Let me tell you about ";
                            break;
                    }

                    return sentimentIntro + topic + ". " + responses[index];
                }

                return responses[index];
            }

            return "Let me tell you about " + topic + "...";
        }


        private void ProvideRandomTip(string topic, MainWindow window)
        {
            if (topic == null && LastTopic != null)
                topic = LastTopic;

            if (topic != null && _tips.ContainsKey(topic))
            {
                var random = new Random();
                int index = random.Next(_tips[topic].Length);
                ArtDisplay.ShowTipBox(_tips[topic][index], window);
                return;
            }

            var randomTopic = _tips.Keys.ElementAt(new Random().Next(_tips.Count));
            ArtDisplay.ShowTipBox(_tips[randomTopic][new Random().Next(_tips[randomTopic].Length)], window);
        }

        private void ProvideFallbackResponse(string input, MainWindow window)
        {
            var favorite = GetFavoriteTopic();
            if (favorite != null && new Random().Next(3) == 0)
            {
                window.AppendToChat($"🤔 Not sure I follow. Maybe we could continue discussing {favorite}?", Brushes.Yellow);
                window.AppendToChat($"Or ask about: {string.Join(", ", GetRandomTopics(3))}", Brushes.Yellow);
            }
            else if (_discussedTopics.Count > 0)
            {
                window.AppendToChat($"🤖 I'm not quite sure what you mean. We recently talked about {_discussedTopics.Last()}.", Brushes.Yellow);
                window.AppendToChat($"Other topics include: {string.Join(", ", GetRandomTopics(4))}", Brushes.Yellow);
            }
            else
            {
                window.AppendToChat("🤖 I'm not sure I understand. Try asking about:", Brushes.Yellow);
                window.AppendToChat("🔹 " + string.Join("\n🔹 ", GetRandomTopics(5)), Brushes.Yellow);
            }
        }

        private List<string> GetRandomTopics(int count)
        {
            return _responses.Keys
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToList();
        }
    }
}