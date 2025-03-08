using System.Text;

namespace Possibilities {
    public partial class Form1 : Form {

        List<String> answers = new List<string>();
        List<String> answers2 = new List<string>();
        List<String> nonanswers = new List<String>();

        String bigInput = "";

        bool solved = false;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            updatePreview();
        }

        private void reveal() {
            for (int i = 0; i < 6; i++) {
                Label score = this.Controls.Find("s" + i, true).OfType<Label>().Single();
                score.Visible = false;
            }
            answers.Clear();
            nonanswers.Clear();
            answers2.Clear();
            String wordleFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Wordle/";
            foreach (string line in File.ReadLines(wordleFolder + "answers.txt")) {
                answers.Add(line);
                answers2.Add(line);
            }
            foreach (string line in File.ReadLines(wordleFolder + "nonanswers.txt")) {
                nonanswers.Add(line);
            }
            String[] guesses = arrayOfGuesses();
            answers = remaining(answers, guesses, 0);
            nonanswers = remaining(nonanswers, guesses, 0);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            for (int i = 0; i < answers.Count; i++) {
                listBox1.Items.Add(answers[i]);
            }
            for (int i = 0; i < nonanswers.Count; i++) {
                listBox2.Items.Add(nonanswers[i]);
            }
            label1.Text = "Answers: " + answers.Count;
            label2.Text = "Nonanswers: " + nonanswers.Count;
            updatePreview();
            solved = false;
            workScores();
        }

        private String[] arrayOfGuesses() {
            String[] guesses = new String[bigInput.Length / 10];
            for (int i = 0; i < bigInput.Length / 10; i++) {
                guesses[i] = bigInput.Substring(i * 10, 5);
            }
            return guesses;
        }

        List<String> remaining(List<String> words, String[] guesses, int helper) {
            for (int i = 0; i < guesses.Length; i++) {
                String guess = guesses[i].ToLower();
                String solutionPattern = bigInput.Substring((helper * 10 + (i*10+5)),5).ToUpper();
                List<String> candidates = new List<String>();
                for (int j = 0; j < words.Count; j++) {
                    if (solutionPattern.Equals(pattern(guess, words[j]))) {
                        candidates.Add(words[j]);
                    }
                }
                words = candidates;
            }
            return words;
        }

        private string pattern(string guess, string solution) {
            StringBuilder pattern = new StringBuilder("00000");
            Dictionary<char, int> letterCounts = getCounts(solution);
            for (int i = 0; i < guess.Length; i++) {
                char letter = guess[i];
                int countOfLetter = letterCounts[letter];
                if (countOfLetter == 0) {
                    pattern[i] = 'X';
                } else {
                    if (solution[i] == guess[i]) {
                        pattern[i] = 'G';
                        letterCounts[letter] = countOfLetter - 1;
                    } else {
                        pattern[i] = 'X';
                    }

                }
            }
            for (int i = 0; i < guess.Length; i++) {
                char letter = guess[i];
                int countOfLetter = letterCounts[letter];
                if (countOfLetter != 0) {
                    if (solution[i] != guess[i]) {
                        pattern[i] = 'Y';
                        letterCounts[letter] = countOfLetter - 1;
                    }
                }
            }
            return pattern.ToString();
        }

        private Dictionary<char, int> getCounts(string solution) {
            Dictionary<char, int> letterCounts = new Dictionary<char, int>();
            for (int i = (int)'a'; i < ((int)'z' + 1); i++) {
                letterCounts.Add((char)i, 0);
            }
            for (int i = 0; i < solution.Length; i++) {
                char c = solution[i];
                letterCounts[c] = letterCounts[c] + 1;
            }
            return letterCounts;
        }

        private void workScores() {
            for (int i = 0; i < (bigInput.Length / 10); i++) {
                int countBefore = answers2.Count;
                answers2 = remaining(answers2, new String[] {bigInput.Substring(i * 10, 5)}, i);
                int countAfter = answers2.Count;
                Label score = this.Controls.Find("s" + i, true).OfType<Label>().Single();
                score.AutoSize = false;
                score.TextAlign = ContentAlignment.MiddleRight;
                updateScore(score, countBefore, countAfter);
            }
        }

        private void updateScore(Label score, int countBefore, int countAfter) {
            double countB = (double)countBefore;
            double countA = (double)countAfter;
            double quotient = countB / countA;
            double logResult = Math.Log2(quotient);
            double logTotal = Math.Log2(countB);
            double scoreNumber = logResult / logTotal;
            scoreNumber = 1 / (1 + Math.Pow(2.71828,(9.22 * (-scoreNumber + 0.5))));
            scoreNumber = (Math.Ceiling(scoreNumber * 100)) / 100;
            try {
                String name = score.Name.Substring(1, 1);
                Label l = this.Controls.Find("l" + name + "0", true).OfType<Label>().Single();
                String row = l.Name.Substring(1, 1);
                int rowNumber = Int32.Parse(row);
                score.ForeColor = Color.White;
                score.BackColor = l.BackColor;
                score.Text = (int)(scoreNumber * 100) + "";
                if (!solved) {
                    if (countAfter == countBefore) {
                        score.Text = "0";
                    }
                    if (countAfter == 0) {
                        score.Text = "";
                    }
                    if (score.Text.Equals("100")) {
                        if (!bigInput.Substring(rowNumber * 10, 5).Equals(listBox1.Items[0].ToString())) {
                            score.Text = "99";
                        }
                    }
                    if (bigInput.Substring(rowNumber * 10, 5).Equals(listBox1.Items[0].ToString()) && listBox1.Items.Count == 1 && bigInput.Substring(rowNumber * 10 + 5, 5).Equals("ggggg")) {
                        score.Text = "100";
                        solved = true;
                    }
                } else {
                    score.Text = "0";
                }
                score.Visible = true;
            } catch {}
        }

        private void reset() {
            bigInput = "";
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            label1.Text = "Answers: ";
            label2.Text = "Nonanswers: ";
            for (int i = 0; i < 6; i++) {
                Label score = this.Controls.Find("s" + i, true).OfType<Label>().Single();
                score.Visible = false;
            }
            updatePreview();
        }

        private void updatePreview() {
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 6; j++) {
                    Color col = ColorTranslator.FromHtml("#ffffff");
                    Label t = this.Controls.Find("l" + j + i, true).OfType<Label>().Single();
                    t.BackColor = col;
                    t.ForeColor = Color.White;
                    t.Text = " ";
                }
            }
            for (int i = 0; i < bigInput.Length; i++) {
                int mod = i % 10;
                Label t;
                if (mod >= 5) {
                    t = this.Controls.Find("l" + ((i - 5) / 10) + (mod - 5), true).OfType<Label>().Single();
                    if (bigInput[i] == 'g') {
                        t.BackColor = ColorTranslator.FromHtml("#538d4e");
                    } else if (bigInput[i] == 'y') {
                        t.BackColor = ColorTranslator.FromHtml("#b59f3b");
                    } else if (bigInput[i] == 'x') {
                        t.BackColor = ColorTranslator.FromHtml("#3a3a3c");
                    }
                    t.ForeColor = Color.White;
                } else {
                    t = this.Controls.Find("l" + (i / 10) + mod, true).OfType<Label>().Single();
                    t.ForeColor = Color.Black;
                    t.Text = (bigInput[i] + "").ToUpper();
                }
            }
        }

        private void KeyPressed(object sender, KeyEventArgs e) {
            tableLayoutPanel1.Focus();
            if (e.KeyCode == Keys.Back) {
                try {
                    bigInput = bigInput.Substring(0, bigInput.Length - 1);
                } catch { }
            } else if (e.KeyCode == Keys.Escape) {
                reset();
            } else if (e.KeyCode == Keys.Enter) {
                for (int i = 0; i < 6; i++) {
                    Label score = this.Controls.Find("s" + i, true).OfType<Label>().Single();
                    score.Text = "";
                }
                label1.Text = "Answers: ";
                label2.Text = "Nonanswers: ";
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                this.Update();
                if (bigInput.Length % 10 == 0) {
                    reveal();
                } else {
                    listBox1.Items.Add("error");
                }
            } else {
                int keyValue = (int)e.KeyCode;
                if (keyValue >= 0x41 && keyValue <= 0x5A && bigInput.Length < 60) {
                    if (bigInput.Length % 10 >= 5) {
                        if ((int)e.KeyCode == 0x59 || (int)e.KeyCode == 0x58 || (int)e.KeyCode == 0x47) {
                            bigInput += e.KeyCode.ToString().ToLower();
                        }
                    } else {
                        bigInput += e.KeyCode.ToString().ToLower();
                    }
                }
            }
            label3.Text = bigInput;
            updatePreview();
            for (int i = 0; i < 6; i++) {
                Label score = this.Controls.Find("s" + i, true).OfType<Label>().Single();
                String name = score.Name.Substring(1, 1);
                Label l = this.Controls.Find("l" + name + "4", true).OfType<Label>().Single();
                score.ForeColor = Color.White;
                score.BackColor = l.BackColor;
            }
        }
    }
}