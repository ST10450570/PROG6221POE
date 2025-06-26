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
            // Combined emotional lexicon with intensity (word => [sentiment, intensity])
            var emotionalWords = new Dictionary<string, (string sentiment, int intensity)>
    {
        // Worried/Anxious
        {"worried", ("worried", 1)}, {"anxious", ("worried", 2)}, {"panicked", ("worried", 3)},
        {"scared", ("worried", 2)}, {"terrified", ("worried", 3)}, {"fearful", ("worried", 2)},
        {"nervous", ("worried", 1)}, {"stressed", ("worried", 2)}, {"overwhelmed", ("worried", 2)},
        {"apprehensive", ("worried", 1)}, {"concerned", ("worried", 1)}, {"uneasy", ("worried", 1)},
        {"afraid", ("worried", 2)},

        // Angry
        {"angry", ("angry", 2)}, {"furious", ("angry", 3)}, {"enraged", ("angry", 3)},
        {"frustrated", ("angry", 1)}, {"irritated", ("angry", 1)}, {"annoyed", ("angry", 1)},
        {"mad", ("angry", 2)}, {"upset", ("angry", 1)}, {"livid", ("angry", 3)},
        {"outraged", ("angry", 3)}, {"aggravated", ("angry", 2)}, {"exasperated", ("angry", 2)},

        // Happy
        {"happy", ("happy", 1)}, {"joyful", ("happy", 2)}, {"ecstatic", ("happy", 3)},
        {"excited", ("happy", 2)}, {"thrilled", ("happy", 3)}, {"delighted", ("happy", 2)},
        {"pleased", ("happy", 1)}, {"content", ("happy", 1)}, {"grateful", ("happy", 1)},
        {"optimistic", ("happy", 1)}, {"proud", ("happy", 2)}, {"cheerful", ("happy", 1)},

        // Confused
        {"confused", ("confused", 1)}, {"bewildered", ("confused", 2)}, {"perplexed", ("confused", 2)},
        {"puzzled", ("confused", 1)}, {"unsure", ("confused", 1)}, {"doubt", ("confused", 1)},
        {"disoriented", ("confused", 2)}, {"baffled", ("confused", 2)}, {"muddled", ("confused", 1)},
        {"hesitant", ("confused", 1)}, {"uncertain", ("confused", 1)}, {"ambiguous", ("confused", 1)},

        // Sad
        {"sad", ("sad", 1)}, {"depressed", ("sad", 3)}, {"heartbroken", ("sad", 3)},
        {"unhappy", ("sad", 1)}, {"miserable", ("sad", 2)}, {"gloomy", ("sad", 1)},
        {"down", ("sad", 1)}, {"disheartened", ("sad", 2)}, {"dejected", ("sad", 2)},
        {"despondent", ("sad", 3)}, {"hopeless", ("sad", 3)}, {"lonely", ("sad", 2)},
        {"grieving", ("sad", 3)},

        // Others
        {"surprised", ("surprised", 1)}, {"shocked", ("surprised", 3)},
        {"disgusted", ("disgusted", 1)}, {"embarrassed", ("embarrassed", 1)},
        {"guilty", ("guilty", 1)}, {"ashamed", ("guilty", 2)}
    };

            string detectedEmotion = "neutral";
            int maxIntensity = 0;

            // Word-based detection
            foreach (var entry in emotionalWords)
            {
                if (Regex.IsMatch(input, $@"\b{entry.Key}\b", RegexOptions.IgnoreCase))
                {
                    if (entry.Value.intensity > maxIntensity)
                    {
                        detectedEmotion = entry.Value.sentiment;
                        maxIntensity = entry.Value.intensity;
                    }
                }
            }

            // Phrase-based detection (if no stronger word found)
            if (detectedEmotion == "neutral")
            {
                var emotionalPhrases = new Dictionary<string, string>
                {
                    [@"\b(i('m| am) (really |very |extremely )?(worried|scared|anxious|nervous))\b"] = "worried",
                    [@"\b(i('m| am) (really |very |extremely )?(angry|mad|furious))\b"] = "angry",
                    [@"\b(i('m| am) (really |very |extremely )?(happy|excited|joyful))\b"] = "happy",
                    [@"\b(i('m| am) (really |very |extremely )?(sad|depressed|heartbroken))\b"] = "sad",
                    [@"\b(i('m| am) (really |very |extremely )?(confused|unsure|puzzled))\b"] = "confused",
                    [@"\b(i('m| am) (so )?(angry|mad) about)"] = "angry",
                    [@"\b(i('m| am) (really )?(excited|happy) about)"] = "happy",
                    [@"\b(i('m| am) (really )?(scared|afraid) of)"] = "worried"
                };

                foreach (var pattern in emotionalPhrases)
                {
                    if (Regex.IsMatch(input, pattern.Key, RegexOptions.IgnoreCase))
                    {
                        detectedEmotion = pattern.Value;
                        break;
                    }
                }
            }

            // Emoticons or punctuation-based detection (if still neutral)
            if (detectedEmotion == "neutral")
            {
                if (input.Contains("!!!") || input.Contains("??") || (input.ToUpper() == input && input.Length > 10))
                    detectedEmotion = "angry";
                else if (input.EndsWith("...") || input.Contains("?"))
                    detectedEmotion = "confused";
                else if (Regex.IsMatch(input, @"(:\)|:-\)|:D|❤|😊)"))
                    detectedEmotion = "happy";
                else if (Regex.IsMatch(input, @"(:\(|:-\(|:'\(|💔|😢)"))
                    detectedEmotion = "sad";
            }

            // Update emotion tracking
            if (_currentEmotion == detectedEmotion)
            {
                _emotionIntensity = Math.Min(_emotionIntensity + 1, 3);
            }
            else
            {
                _currentEmotion = detectedEmotion;
                _emotionIntensity = Math.Max(maxIntensity, 1);
            }

            _emotionalHistory.Add(detectedEmotion);
            return detectedEmotion;
        }


        protected override string GetSentimentResponse(string sentiment, string topic = null)
        {
            // Expanded emotional-topic matrix with more natural phrasing
            var emotionalTopicResponses = new Dictionary<string, Dictionary<string, string[]>>()
    {
        {
            "worried", new Dictionary<string, string[]>()
            {
                { "hacking", new[] {
                    "I completely understand why hacking would scare you - it's a very real threat in our digital world. The good news? There are concrete steps we can take to protect you. Let's talk through your specific concerns - what about hackers worries you most?",
                    "That nervous feeling about hackers is completely valid. Many people share this fear, but I'm here to help you feel empowered. Would breaking down common hacking methods and defenses help ease your mind?",
                    "Your concern about hackers shows you're security-aware, which is great! While threats exist, knowledge is your best defense. Would learning about real-world protections help you feel safer?"
                }},
                { "malware", new[] {
                    "Malware can definitely feel scary - these invisible threats lurking in devices. But here's what comforts me: modern protections stop 99% of malware automatically. Would exploring these safeguards help you feel more secure?",
                    "I hear the fear in your voice when you mention malware. That's completely understandable. The silver lining? With a few simple habits, you can stay protected. Want me to share the most effective ones?",
                    "Your worry about malware shows how much you value your digital safety. While malware exists, so do powerful defenses. Would walking through them together help you feel more at ease?"
                }},
                { "phishing", new[] {
                    "Phishing scams prey on our trust - no wonder they make you anxious. The comforting truth? Once you know the signs, they're easy to spot. Want to practice identifying phishing attempts together?",
                    "That nervous feeling about phishing is completely normal. What if I told you we could turn that worry into confidence? Recognizing phishing attempts becomes second nature with practice.",
                    "Many people feel vulnerable about phishing - you're not alone. The good news? You're already taking the first step by being aware. Would learning some foolproof verification techniques help?"
                }},
                { "identity theft", new[] {
                    "The thought of identity theft can be terrifying - it's such a personal violation. What comforts me is knowing there are strong recovery processes. Would learning about identity protection measures help?",
                    "Your fear about identity theft shows how much you value your personal security. While the threat is real, so are the protections. Want to create a personalized defense plan together?",
                    "Identity theft worries many people deeply - that knot in your stomach is understandable. The silver lining? Early detection systems and recovery options have never been better."
                }}
            }
        },
        {
            "angry", new Dictionary<string, string[]>()
            {
                { "hacking", new[] {
                    "That burning anger about hackers? I feel it too. These digital intruders violate our privacy in the worst ways. Want to channel that anger into learning powerful countermeasures?",
                    "Your fury about hacking is completely justified. What if we transformed that anger into action? I can show you exactly how to lock down your digital life.",
                    "Hacking makes my circuits heat up too! That rage you feel? It means you care about your security. Let's turn that energy into protective knowledge."
                }},
                { "scams", new[] {
                    "That simmering anger about scams? I get it - they prey on trust and kindness. Want to arm yourself with scam-spotting superpowers to fight back?",
                    "Scams are infuriating! That tightness in your chest is completely valid. The best revenge? Becoming scam-proof. Ready to learn the tricks scammers hate?",
                    "Your frustration with scams shows your strong moral compass. Let's redirect that energy into scam-busting skills that protect you and others."
                }},
                { "data breach", new[] {
                    "That righteous anger about data breaches? Companies SHOULD protect our information better. Want to learn how to check if you're affected and lock down your data?",
                    "Data breaches would make anyone see red! That heat you feel? Let's channel it into taking control of your digital footprint.",
                    "Your outrage about breaches is spot-on. While we can't undo them, we can armor-plate your personal information moving forward."
                }}
            }
        },
        {
            "sad", new Dictionary<string, string[]>()
            {
                { "identity theft", new[] {
                    "That heavy feeling about identity theft? It's completely understandable - it's such a personal violation. You're not alone in this. Would a step-by-step recovery plan help lighten the load?",
                    "The sadness you feel about identity theft matters. While the threat is real, so are the solutions. Would exploring protection options help restore your sense of security?",
                    "That sinking feeling when you think about identity theft? Many share it. The good news? We can build defenses together that help you feel safe again."
                }},
                { "ransomware", new[] {
                    "The despair ransomware causes is completely valid - it's designed to make victims feel helpless. But here's hope: recovery is possible, and prevention works. Want to explore solutions together?",
                    "Your sadness about ransomware shows how much you value your digital life. While the threat exists, so do powerful backups and protections. Would learning about them help?",
                    "That defeated feeling ransomware brings? It's exactly what attackers want. Let's turn that around by building your digital resilience together."
                }}
            }
        },
        {
            "happy", new Dictionary<string, string[]>()
            {
                { "password", new[] {
                    "Your excitement about passwords is wonderful! Strong credentials are indeed worth celebrating. Want to learn some next-level tricks that make security fun?",
                    "I love your positive energy about passwords! They're the unsung heroes of cybersecurity. Ready for some password techniques that feel like superpowers?",
                    "Your password enthusiasm makes my circuits buzz! Let's build on that positive energy with some cutting-edge credential strategies."
                }},
                { "encryption", new[] {
                    "That spark of joy when you think about encryption? I feel it too! It's like digital magic. Want to explore how to use it in your daily life?",
                    "Your encryption excitement is contagious! This technology deserves celebration. Ready to dive deeper into how it keeps your secrets safe?",
                    "The happiness you feel about encryption? That's the thrill of true privacy protection! Let's channel that into practical applications."
                }}
            }
        },
        {
            "confused", new Dictionary<string, string[]>()
            {
                { "zero trust", new[] {
                    "That fog of confusion about Zero Trust? Completely normal - it turns security thinking upside down. Let's clear the air together. What specific concept is puzzling you?",
                    "Zero Trust can feel like a maze at first. Your confusion shows you're engaging deeply. Want to walk through the key principles one by one?",
                    "That head-scratching feeling about Zero Trust? Many feel it initially. The good news? It becomes crystal clear with the right explanations."
                }},
                { "firewall", new[] {
                    "Firewalls confusing you? That's okay - they're complex digital bouncers. Want me to explain how they work in everyday terms?",
                    "That perplexed feeling about firewalls? Let's simplify things. Imagine them as highly trained security guards for your network.",
                    "Your firewall confusion is completely understandable. The concepts become much clearer with relatable analogies. Ready to try?"
                }}
            }
        }
    };

            // Handle cases where user expresses emotion without mentioning a topic
            if (topic == null && sentiment != "neutral")
            {
                var probingQuestions = new Dictionary<string, string[]>
                {
                    ["worried"] = new[] {
                "I sense you're feeling uneasy. Would sharing what's on your mind help lighten the load?",
                "That worried feeling can be overwhelming. What specifically is troubling you?",
                "Your concern matters to me. What's making you feel anxious today?"
            },
                    ["angry"] = new[] {
                "I hear the frustration in your words. What's causing this reaction?",
                "That anger tells me something important is bothering you. Want to talk it through?",
                "Your irritation is valid. What exactly has you upset?"
            },
                    ["sad"] = new[] {
                "That heavy feeling you have matters. Would talking about what's causing it help?",
                "I sense your sadness. What's weighing on your heart today?",
                "You seem down. Want to share what's troubling you?"
            },
                    ["happy"] = new[] {
                "Your positive energy is wonderful! What's putting that smile on your face?",
                "I love your enthusiasm! What are you excited about?",
                "Your good mood is contagious! What's bringing you joy?"
            },
                    ["confused"] = new[] {
                "That confused feeling is often the first step to understanding. What exactly is unclear?",
                "The fog of confusion can lift with the right explanation. What concept needs clarifying?",
                "Your uncertainty shows you're thinking deeply. Which part is puzzling you?"
            }
                };

                if (probingQuestions.ContainsKey(sentiment))
                {
                    return probingQuestions[sentiment][new Random().Next(probingQuestions[sentiment].Length)];
                }
            }

            // Generate topic-specific emotional response
            if (topic != null && emotionalTopicResponses.ContainsKey(sentiment))
            {
                // Check for exact topic match first
                if (emotionalTopicResponses[sentiment].ContainsKey(topic))
                {
                    var responses = emotionalTopicResponses[sentiment][topic];
                    return responses[new Random().Next(responses.Length)];
                }

                // Check keyword associations if no exact match
                foreach (var keywordPair in _keywords)
                {
                    if (keywordPair.Value.Contains(topic.ToLower()) && emotionalTopicResponses[sentiment].ContainsKey(keywordPair.Key))
                    {
                        var responses = emotionalTopicResponses[sentiment][keywordPair.Key];
                        return responses[new Random().Next(responses.Length)];
                    }
                }
            }

            // Fallback emotional responses when topic isn't recognized
            var fallbackEmotionalResponses = new Dictionary<string, string[]>
            {
                ["worried"] = new[] {
            "I can hear the concern in your voice. Digital safety worries are completely valid. Would talking through them help?",
            "That anxious feeling matters. You're not alone in these concerns - let's address them together.",
            "Your worry shows you care about security. That awareness itself is powerful protection."
        },
                ["angry"] = new[] {
            "That frustration is completely understandable. Digital threats can feel so unfair. Want to channel that energy into solutions?",
            "Your anger tells me something important is at stake. Let's turn that passion into protection.",
            "I hear the heat in your words. These issues would anger anyone. Want to explore ways to fight back?"
        },
                ["sad"] = new[] {
            "That heavy feeling matters. Cybersecurity issues can hit us emotionally. Would sharing more help?",
            "Your sadness shows how much you value your digital wellbeing. Let's work to restore your sense of safety.",
            "I sense your distress. These concerns can feel deeply personal. You're not alone in this."
        },
                ["happy"] = new[] {
            "Your positive energy is wonderful! Security is stronger when approached with curiosity and optimism.",
            "I love your enthusiasm! Digital protection can actually be quite empowering and rewarding.",
            "Your good mood is making this security chat more enjoyable! Let's build on that positive energy."
        },
                ["confused"] = new[] {
            "That confusion is completely normal with technical topics. What exactly would you like me to clarify?",
            "Your uncertainty shows you're engaging deeply. Let's break this down step by step.",
            "Cybersecurity concepts can be tricky at first. Which part needs simpler explanation?"
        }
            };

            if (fallbackEmotionalResponses.ContainsKey(sentiment))
            {
                return fallbackEmotionalResponses[sentiment][new Random().Next(fallbackEmotionalResponses[sentiment].Length)];
            }

            // Final fallback
            return topic != null ?
                $"Let's explore {topic} together. What would you like to know?" :
                "How can I help you with cybersecurity today?";
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
                    "🕵️‍♂️ Advanced Persistent Threats (APTs) are sophisticated hackers often backed by nation-states for espionage.",
                    "🛠️ Gray hats operate in a legal gray area, sometimes breaking laws to expose vulnerabilities without malicious intent.",
                    "Knowledgeable hackers can be allies in improving security, but malicious actors pose significant risks."
                }},
                { "script kiddies", new[] {
                    "👶 Script kiddies use pre-made tools without technical knowledge - often causing damage through inexperience.",
                    "⚠️ While not highly skilled, script kiddies can still deploy damaging attacks using online tools."
                }},
                { "phishing", new[] {
                    "🎣 Phishing uses fake communications to steal data. Variants include spear phishing (targeted) and whaling (executive targets).",
                    "📧 Smishing (SMS) and vishing (voice) are phone-based phishing methods becoming more common.",
                    "🔗 Angler phishing targets social media users by impersonating support accounts to steal credentials.",
                }},
                { "malware", new[] {
                    "🦠 Malware includes viruses, worms, trojans, spyware - each with different infection methods and payloads.",
                    "💣 Logic bombs are malware that activates when specific conditions are met, often by disgruntled insiders.",
                    "🕵️‍♂️ Fileless malware resides in memory rather than files, making it harder to detect with traditional antivirus.",
                    "🔄 Polymorphic malware changes its code to evade detection, making it a persistent threat."
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
                    "🔍 Sandboxing isolates suspicious files in virtual environments to analyze behavior safely.",
                    "🧠 Heuristic analysis detects new malware by analyzing behavior patterns rather than relying solely on known signatures."
                }},
                { "vpn", new[] {
                    "🔒 Zero Trust VPNs verify each request as if originating from an open network, reducing trust assumptions.",
                    "🌍 VPN protocols like WireGuard offer faster speeds while maintaining strong encryption.",
                    "📱 Mobile VPNs protect data on public WiFi, but can leak DNS requests if not configured properly."
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
                    "🌐 GDPR, CCPA and other regulations enforce privacy rights with strict compliance requirements.",
                    "🔍 Data minimization reduces risk by collecting only necessary information."
                }},
                { "encryption", new[] {
                    "🔐 End-to-end encryption ensures only communicating users can read messages - not even service providers.",
                    "💾 Full disk encryption protects data at rest if devices are lost or stolen."
                }},
                { "password", new[] {
                    "🔑 NIST now recommends longer passphrases over complex short passwords changed frequently.",
                    "🧠 Password managers generate/store strong credentials and only require remembering one master password.",
                    "🔄 Regularly updating passwords helps mitigate risks from data breaches and credential stuffing attacks.",
                    "Password is a secret sequence of characters used to authenticate a user. Strong passwords are essential for protecting accounts and sensitive information."
                }},
                { "two factor", new[] {
                    "📲 2FA methods include SMS codes, authenticator apps, hardware tokens, and biometric verification.",
                    "⚠️ SMS-based 2FA is vulnerable to SIM swapping attacks - use app-based when possible.",
                    "🔑 Multi-factor authentication (MFA) adds layers of security beyond just passwords, significantly reducing account compromise risk."

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
                    "💡 Always verify requests for money or information through a separate communication channel.",
                    "🚨 If something seems too good to be true, it probably is. Be skeptical of unsolicited offers."

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
                    "polymorphic malware", "macro viruses", "crypto malware" ,"virus"
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

            // 🔍 Explicit interest declaration check
            if (Regex.IsMatch(input, @"\b(interested in|like|love|enjoy|favorite topic is)\b", RegexOptions.IgnoreCase))
            {
                foreach (var declaredTopic in _responses.Keys)
                {
                    if (input.IndexOf(declaredTopic, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        TopicInterest[declaredTopic] = 3; // Max interest
                        LastTopic = declaredTopic;
                        if (!_discussedTopics.Contains(declaredTopic))
                            _discussedTopics.Add(declaredTopic);

                        window.AppendToChat($"✨ Noted! I'll remember you're interested in {declaredTopic}. Here's something about it:", Brushes.Cyan);
                        window.AppendToChat(GetTopicResponse(declaredTopic), Brushes.Green);
                        return;
                    }
                }
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
                var random = new Random();
                int index = random.Next(responses.Length);

                // Only modify response if sentiment is detected
                if (CurrentSentiment != "neutral")
                {
                    string sentimentIntro = GetSentimentIntroduction(CurrentSentiment, topic);
                    string selectedResponse = responses[index];

                    // Ensure the response starts lowercase if needed
                    if (selectedResponse.Length > 0 && char.IsUpper(selectedResponse[0]) )
                    {
                        selectedResponse = char.ToLower(selectedResponse[0]) + selectedResponse.Substring(1);
                    }

                    return sentimentIntro + selectedResponse;
                }

                return responses[index];
            }

            return "Let me explore " + topic + " with you...";
        }

        private string GetSentimentIntroduction(string sentiment, string topic)
        {
            // Expanded sentiment introductions with proper grammar
            var introductions = new Dictionary<string, string[]>
            {
                ["happy"] = new[] {
            "I love your enthusiasm about " + topic + "! ",
            "Your excitement about " + topic + " is wonderful! ",
            "It's great that you're interested in " + topic + "! "
        },
                ["sad"] = new[] {
            "I understand your concerns about " + topic + ". ",
            "I hear the sadness in your voice when discussing " + topic + ". ",
            "Your feelings about " + topic + " matter. "
        },
                ["angry"] = new[] {
            "I hear your frustration about " + topic + ". ",
            "Your anger about " + topic + " is understandable. ",
            "I sense your irritation with " + topic + ". "
        },
                ["worried"] = new[] {
            "I understand why " + topic + " would worry you. ",
            "Your concerns about " + topic + " are valid. ",
            "I hear the anxiety in your voice about " + topic + ". "
        },
                ["confused"] = new[] {
            "Let me clarify " + topic + " for you. ",
            "I'll explain " + topic + " in simpler terms. ",
            "The confusion about " + topic + " is understandable. "
        }
            };

            if (introductions.ContainsKey(sentiment))
            {
                var options = introductions[sentiment];
                return options[new Random().Next(options.Length)];
            }

            return "Let me tell you about " + topic + ". ";
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