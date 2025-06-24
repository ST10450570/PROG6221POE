using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Chatbot
{
    public abstract class ChatbotBase
    {
        public string Username { get; set; }
        public string AudioPath { get; set; }
        protected Dictionary<string, int> TopicInterest { get; set; }
        protected string LastTopic { get; set; }
        protected string CurrentSentiment { get; set; }

        public ChatbotBase(string username, string audioPath)
        {
            Username = string.IsNullOrEmpty(username) ? "Friend" : username;
            AudioPath = audioPath;
            TopicInterest = new Dictionary<string, int>();
            LastTopic = null;
            CurrentSentiment = "neutral";
        }

        public ChatbotBase(string audioPath) : this("Friend", audioPath) { }

        public abstract void Greet(MainWindow window);
        public abstract void StartChat();

        protected virtual string DetectSentiment(string input)
        {
            var negativeWords = new List<string> {
                "worried", "anxious", "scared", "nervous", "afraid", "concerned",
                "fearful", "stressed", "uneasy", "apprehensive", "panicked", "terrified"
            };

            var angryWords = new List<string> {
                "angry", "frustrated", "mad", "annoyed", "upset", "irritated",
                "furious", "enraged", "aggravated", "exasperated", "outraged", "livid"
            };

            var positiveWords = new List<string> {
                "happy", "excited", "joyful", "great", "awesome", "cool",
                "pleased", "delighted", "thrilled", "ecstatic", "content", "grateful"
            };

            var confusedWords = new List<string> {
                "confused", "unsure", "doubt", "don't understand", "puzzled",
                "bewildered", "perplexed", "baffled", "disoriented", "muddled"
            };

            var sadWords = new List<string> {
                "sad", "depressed", "unhappy", "miserable", "heartbroken",
                "gloomy", "down", "disheartened", "dejected", "despondent"
            };

            foreach (var word in negativeWords)
                if (Regex.IsMatch(input, $@"\b{word}\b", RegexOptions.IgnoreCase)) return "worried";

            foreach (var word in angryWords)
                if (Regex.IsMatch(input, $@"\b{word}\b", RegexOptions.IgnoreCase)) return "angry";

            foreach (var word in positiveWords)
                if (Regex.IsMatch(input, $@"\b{word}\b", RegexOptions.IgnoreCase)) return "happy";

            foreach (var word in confusedWords)
                if (Regex.IsMatch(input, $@"\b{word}\b", RegexOptions.IgnoreCase)) return "confused";

            foreach (var word in sadWords)
                if (Regex.IsMatch(input, $@"\b{word}\b", RegexOptions.IgnoreCase)) return "sad";

            if (input.Contains("!!!") || input.Contains("??") || (input.ToUpper() == input && input.Length > 10))
                return "angry";

            if (input.EndsWith("...") || input.Contains("?"))
                return "confused";

            if (input.Contains(":)") || input.Contains(":-)") || input.Contains(":D"))
                return "happy";

            if (input.Contains(":(") || input.Contains(":-(") || input.Contains(":'("))
                return "sad";

            return "neutral";
        }

        protected virtual string GetSentimentResponse(string sentiment, string topic = null)
        {
            var responses = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "worried", new Dictionary<string, string>()
                    {
                        { "phishing", "I understand phishing can be really scary with all those sophisticated fake emails going around. The good news is you're taking the first step by learning how to spot them! Would you like me to walk you through some recent phishing examples so you can feel more prepared?" },
                        { "malware", "Malware does sound frightening, especially with how sneaky some variants can be. But here's something reassuring - with proper precautions, you can keep your devices about 99% safer. Would you like to know the most effective protections against malware?" },
                        { "hacking", "The idea of being hacked is definitely anxiety-inducing. I want you to know that while threats exist, there are many powerful ways to protect yourself. What specifically about hacking worries you the most?" },
                        { "default", "I can hear the concern in your words. Cybersecurity issues can feel overwhelming, but you're not alone in this. Would it help if we talk through what's worrying you? I'm here to help you feel more secure." }
                    }
                },
                {
                    "angry", new Dictionary<string, string>()
                    {
                        { "hacking", "It's completely understandable to feel furious about hacking - these criminals violate our privacy and create real harm. That anger shows you care about your digital safety. Would venting about hacking experiences help, or would you prefer solutions to protect yourself?" },
                        { "scam", "Scams are absolutely infuriating! They prey on trust and can hit anyone. That anger you feel? It's justified. The best revenge is arming yourself with knowledge to shut scammers down. Want me to share the most effective scam-busting techniques?" },
                        { "data breach", "Data breaches make me angry too! Companies should protect our information better. I share your frustration when organizations fail at basic security. Would you like to know how to check if your data was exposed in recent breaches?" },
                        { "default", "I hear the frustration in your voice. Cybersecurity issues can be incredibly aggravating - they often feel unfair. Would talking about what's upsetting you help? I'm ready to listen and help where I can." }
                    }
                },
                {
                    "happy", new Dictionary<string, string>()
                    {
                        { "password", "Your enthusiasm about password security is contagious! Strong credentials are indeed one of the most powerful ways to protect yourself. Would you like to learn some advanced password techniques that even most professionals don't know?" },
                        { "vpn", "Your positive attitude about VPNs is fantastic! They're such powerful tools for privacy. I'm excited to share some next-level VPN tips with someone who appreciates them as much as you do!" },
                        { "encryption", "It's wonderful to see someone so excited about encryption! This technology is like a superpower for privacy. Would you like me to explain how you can easily implement encryption in your daily digital life?" },
                        { "default", "Your positive energy is making this cybersecurity chat much more enjoyable! Is there a particular security topic you're excited to explore more deeply? I'd love to nurture that interest." }
                    }
                },
                {
                    "confused", new Dictionary<string, string>()
                    {
                        { "encryption", "Encryption can definitely be confusing at first - it's like a secret code language for computers. Let me break it down in simple terms. What specific part about encryption is puzzling you?" },
                        { "firewall", "Firewalls seem complex at first glance, but think of them like bouncers at a club deciding who gets in. Could you tell me which aspect of firewalls is unclear? I'll explain it in everyday language." },
                        { "zero trust", "Zero Trust does have some counterintuitive concepts. The 'never trust, always verify' approach is different from traditional security. What part would you like me to clarify first?" },
                        { "default", "I sense some confusion in your question, and that's completely normal with cybersecurity topics. Could you help me understand which part isn't clear so I can explain it better?" }
                    }
                },
                {
                    "sad", new Dictionary<string, string>()
                    {
                        { "identity theft", "Identity theft can leave you feeling violated and vulnerable - those feelings are completely valid. The good news is there are clear steps to recover and protect yourself. Would you like me to guide you through them?" },
                        { "ransomware", "Ransomware attacks can be devastating, leaving people feeling helpless. I want you to know there are ways to fight back and protect your data. Would learning about ransomware recovery options help?" },
                        { "default", "I hear the sadness in your words, and I want you to know your feelings matter. Cybersecurity issues can sometimes feel personal and upsetting. Would talking through what's troubling you help?" }
                    }
                },
                {
                    "neutral", new Dictionary<string, string>()
                    {
                        { "default", topic != null ? $"Let's explore {topic} together. What would you like to know about it?" : "How can I help you with cybersecurity today?" }
                    }
                }
            };

            if (sentiment != "neutral" && topic == null)
            {
                string response;
                switch (sentiment)
                {
                    case "worried":
                        response = "I sense you're feeling concerned about something. Could you tell me what cybersecurity aspect is worrying you?";
                        break;
                    case "angry":
                        response = "I can hear the frustration in your words. What cybersecurity issue is making you feel this way?";
                        break;
                    case "sad":
                        response = "You seem upset. Would sharing what's bothering you related to online security help?";
                        break;
                    case "happy":
                        response = "You sound positive! Is there a particular cybersecurity topic you're excited about?";
                        break;
                    case "confused":
                        response = "I sense some confusion. What cybersecurity concept would you like me to clarify?";
                        break;
                    default:
                        response = "How can I help you with cybersecurity today?";
                        break;
                }

                return response;
            }


            if (responses.ContainsKey(sentiment))
            {
                if (topic != null && responses[sentiment].ContainsKey(topic))
                    return responses[sentiment][topic];
                return responses[sentiment]["default"];
            }

            return responses["neutral"]["default"];
        }

        protected string GetFollowUpQuestion(string sentiment)
        {
            var questions = new Dictionary<string, List<string>>()
            {
                {"worried", new List<string> {
                    "What specifically about this is worrying you?",
                    "Would sharing your concerns help you feel better?",
                    "What aspect of this makes you most anxious?"
                }},
                {"angry", new List<string> {
                    "What exactly has you so upset?",
                    "Would venting about the situation help?",
                    "What triggered this reaction?"
                }},
                {"sad", new List<string> {
                    "Would talking about what happened help?",
                    "What's making you feel this way?",
                    "Do you want to share what's troubling you?"
                }},
                {"overwhelmed", new List<string> {
                    "What part feels most overwhelming?",
                    "Would breaking this down help?",
                    "What's the most pressing concern?"
                }},
                {"confused", new List<string> {
                    "What exactly is unclear?",
                    "Which part would you like me to explain differently?",
                    "What concept is most confusing?"
                }},
                {"happy", new List<string> {
                    "What's putting you in such a good mood?",
                    "What are you most excited about?",
                    "What positive experience would you like to share?"
                }}
            };

            if (questions.ContainsKey(sentiment))
            {
                return questions[sentiment][new Random().Next(questions[sentiment].Count)];
            }

            return "Can you tell me more about what's on your mind?";
        }

        protected void TrackTopicInterest(string topic)
        {
            if (TopicInterest.ContainsKey(topic))
                TopicInterest[topic]++;
            else
                TopicInterest[topic] = 1;
        }

        protected string GetFavoriteTopic()
        {
            if (TopicInterest.Count == 0) return null;

            var maxInterest = 0;
            string favorite = null;

            foreach (var kvp in TopicInterest)
            {
                if (kvp.Value > maxInterest)
                {
                    maxInterest = kvp.Value;
                    favorite = kvp.Key;
                }
            }

            return maxInterest >= 3 ? favorite : null;
        }
    }
}