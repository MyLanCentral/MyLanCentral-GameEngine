using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Mylancentral {
    namespace Framework {
        /* Framework Members and Methods */
        public static class Methods {
            /*public static int FirstAvailableID(List<int> current_ids) {
                List<int> current, expected;
                IEnumerable<int> missing;

                current = new List<int>(current_ids);
                expected = new List<int>();

                if (current.Count <= 0) {
                    return 0;
                }

                for (int i = 0; i < current.Count; i++) {
                    expected.Add(i);
                }

                missing = expected.Except(current);

                if (missing.Count() > 0) {
                    return missing.ElementAt(0);
                }
                else {
                    return current.Count;
                }
            }*/

            public static int FirstAvailableID(IEnumerable<Entity> entity_list) {
                List<int> current, expected;
                IEnumerable<int> missing;

                current = new List<int>();
                expected = new List<int>();

                if (current.Count <= 0) {
                    return 0;
                }
                else {
                    foreach(Entity entity in entity_list) {
                        current.Add(entity.ID);
                    }
                }

                for (int i = 0; i < current.Count; i++) {
                    expected.Add(i);
                }

                missing = expected.Except(current);

                if (missing.Count() > 0) {
                    return missing.ElementAt(0);
                }
                else {
                    return current.Count;
                }
            }
        }
        public abstract class Entity {
            public int ID { get; protected set; }
            public string Name { get; protected set; }
            public Vector2 Position { get; protected set; }
            public Vector2 Velocity { get; protected set; }
            public Vector2 Origin { get; protected set; }
            public Texture2D Texture { get; protected set; }
            public Color Color { get; protected set; }
            public Rectangle BoundaryBox { get; protected set; }
            public float Scale { get; protected set; }
            public float Rotation { get; protected set; }
            public float MoveRate { get; protected set; }
            public bool DestroyMe { get; protected set; }

            /// <summary>
            /// Initializes Entity with required information and 
            /// nulls other variables until manually set
            /// </summary>
            /// <param name="id">Integer Identifier</param>
            /// <param name="blend_color">Color used to blend with the texture (default Color.White)</param>
            /// <param name="texture">Texture used for the Entity</param>
            /// <param name="position">Position of the object</param>
            /// <param name="scale">Scale of the Texture (default 1)</param>
            /// <param name="rotation">Rotation of Texture (default 0)</param>
            /// <param name="move_rate">Rate of Motion</param>
            protected void InitializeEntity(int id, Color blend_color, Texture2D texture, Vector2 position, float scale, float rotation, float move_rate) {
                ID = id;
                Color = blend_color;
                Texture = texture;
                Position = position;
                Scale = scale;
                Rotation = rotation;
                MoveRate = move_rate;

                Name = "";
                Velocity = new Vector2(0);

                Origin = SetOrigin();
                BoundaryBox = CreateCollisionRectangle();
                DestroyMe = false;
            }

            /// <summary>
            /// Creates and Returns a Rectangle based off
            /// Position and Texture size
            /// </summary>
            /// <returns></returns>
            private Rectangle CreateCollisionRectangle() {
                Rectangle rect;
                Point position, size;

                position = new Point((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y);
                size = new Point(Texture.Width, Texture.Height);

                rect = new Rectangle(position, size);

                return rect;
            }

            /// <summary>
            /// Sets Origin of the Entity based
            /// on Texture size
            /// </summary>
            /// <returns></returns>
            private Vector2 SetOrigin() {
                Vector2 origin;

                origin = new Vector2((float)Texture.Width / 2, (float)Texture.Height / 2);

                return origin;
            }

            /// <summary>
            /// Applies Velocity to Position, then
            /// zeroes out Velocity
            /// </summary>
            protected void ApplyVelocity() {
                Position = Position + Velocity;

                Velocity = new Vector2(0);

                BoundaryBox = CreateCollisionRectangle();
            }

            /// <summary>
            /// Performs a Basic Draw Function using all the pre-set information
            /// </summary>
            /// <param name="spriteBatch"></param>
            protected void DrawSelf(SpriteBatch spriteBatch) {
                spriteBatch.Draw(Texture, Position, null, Color, Rotation, Origin, Scale, SpriteEffects.None, 0);
            }
        }
        public interface IEntity : IDisposable {
            void Initialize();
            void Update(GameTime gameTime);
            void Draw(SpriteBatch spriteBatch);
        }

        namespace SaveLoad {
            public abstract class SaveFile {

            }
            public interface ISaveFile {

            }
        }



        /// <summary>
        /// Derivative Libary of Controls and Classes
        /// used to create and operate Dialog boxes and
        /// messages
        /// </summary>
        namespace DialogEngine {
            public class Choice {
                public int ID { get; protected set; }
                public string Name { get; protected set; }
                public Vector2 Position { get; protected set; }
                public int ReturnValue { get; protected set; }

                public Choice(int id, string name, int return_value) {
                    ID = id;
                    Name = name;
                    ReturnValue = return_value;
                }

                /// <summary>
                /// Sets the correct Vector2 based on the
                /// total number of choices paired with this one
                /// </summary>
                /// <param name="total_choices">Total Number of Choices in the ChoiceDialog</param>
                /// <param name="displaysize">Vector2 containing the screens width/height</param>
                public void SetPosition(int total_choices, Vector2 displaysize) {
                    float cx, cy;

                    cx = (displaysize.X / 2) - ((Name.Length * 8) / 2);
                    cy = (displaysize.Y / 2) - (16 * ((total_choices - 1) / 2)) + (16 * ID);

                    Position = new Vector2(cx, cy);
                }
            }
            public class ChoiceDialog {
                public List<Choice> Choices { get; protected set; }
                public int CurrentChoice { get; protected set; }
                private int LastChoice { get; set; }
                public Choice WidestChoice { get; protected set; }
                private SpriteFont ChoiceFont { get; set; }
                private Color ChoiceColor { get; set; }
                private Rectangle Selection { get; set; }
                private Rectangle DialogRectangle { get; set; }
                private Texture2D ChoiceboxTexture { get; set; }
                private Texture2D SelectionTexture { get; set; }
                public bool DestroyMe { get; protected set; }

                public ChoiceDialog(DialogObject dialog, Texture2D dialog_texture, Texture2D selection_texture, params string[] choices) {
                    Choices = new List<Choice>();
                    ChoiceboxTexture = dialog_texture;
                    SelectionTexture = selection_texture;

                    for (int i = 0; i < choices.Length; i++) {
                        Choice choice = new Choice(i, choices.ElementAt(i), i);
                        Choices.Add(choice);
                    }

                    Initialize(dialog);
                }

                public ChoiceDialog(DialogObject dialog, Texture2D dialog_texture, Texture2D selection_texture, List<Choice> choices) {
                    Choices = new List<Choice>(choices);
                    ChoiceboxTexture = dialog_texture;
                    SelectionTexture = selection_texture;

                    Initialize(dialog);
                }

                public void Initialize(DialogObject dialog) {
                    CurrentChoice = 0;
                    DestroyMe = false;
                    LastChoice = Choices.Count - 1;
                    WidestChoice = Choices.ElementAt(1);
                    ChoiceFont = dialog.MessageFont;
                    ChoiceColor = dialog.MessageColor;
                    //Content.RootDirectory = "Content";
                    //ChoiceboxTexture = Content.Load<Texture2D>("Choicebox");

                    // Find the Choice option that is the widest,
                    // this is used for making sure the ChoiceDialog
                    // box is wide enough
                    foreach (Choice c in Choices) {
                        if (c.Name.Length > WidestChoice.Name.Length) {
                            WidestChoice = c;
                        }
                    }

                    Selection = SetSelectionRectangle(Choices.ElementAt(CurrentChoice));
                    DialogRectangle = CreateChoiceDialogRectangle(WidestChoice, Choices);
                }

                public Rectangle SetSelectionRectangle(Choice choice) {
                    Point location, size;

                    location = new Point((int)(choice.Position.X - 4), (int)(choice.Position.Y - 4));
                    size = new Point((choice.Name.Length * 8) + 4, 12);

                    return new Rectangle(location, size);
                }

                private Rectangle CreateChoiceDialogRectangle(Choice widest_choice, List<Choice> choices) {
                    Rectangle dialogRect;
                    Choice firstChoice = Choices.ElementAt(0);

                    Point location = new Point((int)firstChoice.Position.X - 16, (int)firstChoice.Position.Y - 10);
                    Point size = new Point((int)(WidestChoice.Name.Length * 12) + 12, (Choices.Count * 16) + 12);

                    dialogRect = new Rectangle(location, size);

                    return dialogRect;
                }

                public int SelectChoice() {
                    DestroyMe = true;

                    return Choices.ElementAt(CurrentChoice).ReturnValue;
                }

                public int SelectNext() {
                    if (CurrentChoice == LastChoice) {
                        CurrentChoice = 0;
                    }
                    else {
                        CurrentChoice = CurrentChoice + 1;
                    }

                    Selection = SetSelectionRectangle(Choices.ElementAt(CurrentChoice));

                    return Choices.ElementAt(CurrentChoice).ID;
                }

                public int SelectPrevious() {
                    if (CurrentChoice == 0) {
                        CurrentChoice = LastChoice;
                    }
                    else {
                        CurrentChoice = CurrentChoice - 1;
                    }

                    Selection = SetSelectionRectangle(Choices.ElementAt(CurrentChoice));

                    return Choices.ElementAt(CurrentChoice).ID;
                }

                public void Draw(SpriteBatch spriteBatch) {
                    Choice choice = Choices.ElementAt(CurrentChoice);

                    // DRAW THE CHOICEBOX HERE
                    // --> DO IT
                    spriteBatch.Draw(ChoiceboxTexture, DialogRectangle, Color.White);

                    // DRAW THE CURRENT SELECTION SPRITE HERE
                    // --> DO IT
                    //spriteBatch.Draw(null, Selection, Color.White);
                    spriteBatch.Draw(SelectionTexture, new Vector2(choice.Position.X - 12, choice.Position.Y), Color.White);
                    spriteBatch.Draw(SelectionTexture, new Vector2(choice.Position.X + ((choice.Name.Length * 10) + 20), choice.Position.Y), null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.FlipHorizontally, 0f);

                    foreach (Choice c in Choices) {
                        spriteBatch.DrawString(ChoiceFont, c.Name, c.Position, ChoiceColor);
                    }
                }
            }
            public class DialogBranch {
                public int ID { get; protected set; }
                public List<DialogObject> Dialogs { get; protected set; }
                public int AccessChoice { get; protected set; }
                private int CurrentDialog { get; set; }
                private int LastDialog { get; set; }
                public bool DestroyMe { get; protected set; }
                public bool IsFinalBranch { get; protected set; }
                public bool ReturnsChoice { get; protected set; }

                public DialogBranch(int id, int access_choice, List<DialogObject> dialogs, bool final_branch) {
                    ID = id;
                    AccessChoice = access_choice;
                    Dialogs = new List<DialogObject>(dialogs);
                    IsFinalBranch = final_branch;
                    CurrentDialog = 0;
                    LastDialog = Dialogs.Count - 1;
                    DestroyMe = false;

                    if (Dialogs.ElementAt(LastDialog).ChoiceDialog != null) {
                        ReturnsChoice = true;
                    }
                    else {
                        ReturnsChoice = false;
                    }
                }

                public DialogObject Update() {
                    DialogObject dialog = Dialogs.ElementAt(CurrentDialog);

                    if (dialog.DestroyMe == false) {
                        // Return the Dialog at Current
                        return dialog;
                    }
                    else {
                        if (CurrentDialog < LastDialog) {
                            CurrentDialog = CurrentDialog + 1;
                            return Dialogs.ElementAt(CurrentDialog);
                        }
                        else {
                            return null;
                        }
                    }
                }

                public void Complete() {
                    DestroyMe = true;

                    for (int i = Dialogs.Count - 1; i >= 0; i--) {
                        Dialogs.RemoveAt(i);
                    }
                }
            }
            public class DialogController {
                /// <summary>
                /// Integer Identifier for DialogController
                /// </summary>
                public int ID { get; protected set; }
                /// <summary>
                /// 0 for DialogObject List, 1 for DialogTree
                /// </summary>
                public int DialogMode { get; protected set; }
                /// <summary>
                /// Flag for Deletion by the MainGame
                /// </summary>
                public bool IsComplete { get; protected set; }

                private List<DialogObject> Dialogs { get; set; }
                private DialogTree ActiveDialogTree { get; set; }
                private DialogBranch ActiveDialogBranch { get; set; }
                private DialogObject ActiveDialogObject { get; set; }

                private int CurrentDialogIndex { get; set; }
                private int LastDialogIndex { get; set; }
                public int CurrentChoiceIndex { get; protected set; }

                private float KeyTimer { get; set; }
                private float KeyTimerReset { get; set; }

                public DialogController(int id) {
                    ID = id;

                    Initialize();
                }

                private void Initialize() {
                    DialogMode = 0;

                    Dialogs = new List<DialogObject>();
                    ActiveDialogTree = null;
                    ActiveDialogBranch = null;
                    ActiveDialogObject = null;

                    CurrentDialogIndex = 0;
                    LastDialogIndex = 0;
                    CurrentChoiceIndex = 0;

                    KeyTimer = 0;
                    KeyTimerReset = 0.15f;
                }

                /// <summary>
                /// Sets up the DialogController to Process a single list of
                /// DialogObjects
                /// </summary>
                /// <param name="dialogs">List of DialogObjects to Process</param>
                public void StartDialog(List<DialogObject> dialogs) {
                    DialogMode = 0;
                    Dialogs = new List<DialogObject>(dialogs);
                    CurrentDialogIndex = 0;
                    LastDialogIndex = Dialogs.Count - 1;
                    CurrentChoiceIndex = 0;
                    ActiveDialogObject = Dialogs.ElementAt(CurrentDialogIndex);
                    IsComplete = false;
                }

                /// <summary>
                /// Sets up the DialogController to Process a DialogTree,
                /// Opening the First Branch and its Dialogs to Process
                /// </summary>
                /// <param name="dialog_tree">The Predefined DialogTree to Process</param>
                public void StartDialog(DialogTree dialog_tree) {
                    DialogMode = 1;
                    CurrentChoiceIndex = 999;
                    ActiveDialogTree = dialog_tree;
                    ActiveDialogBranch = ActiveDialogTree.GetBranch(CurrentChoiceIndex);

                    Dialogs = new List<DialogObject>(ActiveDialogBranch.Dialogs);
                    CurrentDialogIndex = 0;
                    LastDialogIndex = Dialogs.Count - 1;

                    ActiveDialogObject = Dialogs.ElementAt(CurrentDialogIndex);
                    IsComplete = false;
                }

                /// <summary>
                /// Updates all Timers associated to this Object
                /// </summary>
                /// <param name="gameTime"></param>
                private void UpdateTimers(GameTime gameTime) {
                    if (KeyTimer > 0) {
                        KeyTimer = KeyTimer - (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }

                private void EmptyDialogsList() {
                    for (int i = Dialogs.Count - 1; i >= 0; i--) {
                        Dialogs.RemoveAt(i);
                    }

                    CurrentDialogIndex = 0;
                }

                private void UpdateDialogsList(GameTime gameTime) {
                    if (ActiveDialogObject.DestroyMe == true) {
                        if (CurrentDialogIndex < LastDialogIndex) {
                            CurrentDialogIndex = CurrentDialogIndex + 1;
                            ActiveDialogObject = Dialogs.ElementAt(CurrentDialogIndex);
                        }
                        else {
                            if (ActiveDialogObject.ChoiceDialog != null) {
                                CurrentChoiceIndex = ActiveDialogObject.SelectedChoice;
                            }

                            EmptyDialogsList();
                            IsComplete = true;
                        }
                    }
                    else {
                        if (KeyTimer <= 0) {
                            if (Keyboard.GetState().IsKeyDown(Keys.Space) == true) {
                                if (ActiveDialogObject.FullMessageDisplayed == false) {
                                    ActiveDialogObject.EnableHold();
                                }
                                else {
                                    if (ActiveDialogObject.Hold == 0) {
                                        ActiveDialogObject.ContinueKey();
                                        KeyTimer = KeyTimerReset;
                                    }
                                }
                            }

                            if (Keyboard.GetState().IsKeyUp(Keys.Space) == true) {
                                if (ActiveDialogObject.Hold == 1) {
                                    ActiveDialogObject.DisableHold();
                                }
                            }
                        }
                    }

                    ActiveDialogObject.Update(gameTime);
                }

                public static string WrapText(SpriteFont font, string text, float maxLineWidth) {
                    string[] words = text.Split(' ');
                    StringBuilder sb = new StringBuilder();
                    float lineWidth = 0f;
                    float spaceWidth = font.MeasureString(" ").X;

                    foreach (string word in words) {
                        Vector2 size = font.MeasureString(word);

                        if (lineWidth + size.X < maxLineWidth) {
                            sb.Append(word + " ");
                            lineWidth += size.X + spaceWidth;
                        }
                        else {
                            if (size.X > maxLineWidth) {
                                if (sb.ToString() == "") {
                                    sb.Append(WrapText(font, word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                                }
                                else {
                                    sb.Append("\n" + WrapText(font, word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                                }
                            }
                            else {
                                sb.Append("\n" + word + " ");
                                lineWidth = size.X + spaceWidth;
                            }
                        }
                    }

                    return sb.ToString();
                }

                public void Update(GameTime gameTime) {
                    KeyboardState keystate = Keyboard.GetState();

                    UpdateTimers(gameTime);

                    if (IsComplete == false) {
                        if (ActiveDialogTree != null) {
                            if (ActiveDialogBranch.DestroyMe == true) {
                                // Move to Next Branch or Flag Tree for
                                // Destruction
                                if (ActiveDialogBranch.IsFinalBranch == true) {
                                    // Close out ActiveDialogTree
                                    ActiveDialogTree.Complete();
                                    IsComplete = true;
                                }
                                else {
                                    // Move to the Next Branch
                                    ActiveDialogBranch = ActiveDialogTree.GetBranch(CurrentChoiceIndex);

                                    Dialogs = new List<DialogObject>(ActiveDialogBranch.Dialogs);
                                    CurrentDialogIndex = 0;
                                    LastDialogIndex = Dialogs.Count - 1;
                                }
                            }
                            else {
                                // Dialog Branch is not flagged for destruction
                                if (Dialogs.Count > 0) {
                                    UpdateDialogsList(gameTime);
                                }
                                else {
                                    ActiveDialogBranch.Complete();
                                }
                            }
                        }
                        else {
                            // Process the Dialogs List normally
                            // if there's no Tree
                            UpdateDialogsList(gameTime);
                        }
                    }
                }

                public void Draw(SpriteBatch spriteBatch) {
                    ActiveDialogObject.Draw(spriteBatch);
                }
            }
            public class DialogObject {
                /// <summary>
                /// Integer Identifier
                /// </summary>
                public int ID { get; protected set; }
                /// <summary>
                /// Color used to draw text
                /// </summary>
                public Color MessageColor { get; protected set; }
                /// <summary>
                /// List of messages to display in the dialog box
                /// </summary>
                public List<string> Messages { get; protected set; }
                /// <summary>
                /// Used during Update to display a SceneImage, if necessary
                /// </summary>
                public bool SceneDriven { get; protected set; }
                /// <summary>
                /// List of Scene Objects used to create cut-scene effects
                /// </summary>
                public List<Scene> Scenes { get; protected set; }
                /// <summary>
                /// Image used for cutscene, if any
                /// </summary>
                public Texture2D SceneImage { get; protected set; }
                /// <summary>
                /// The index of the current scene being shown; compared to LastScene when updating
                /// </summary>
                public int CurrentScene { get; protected set; }
                /// <summary>
                /// Index of the Last Scene in the Scenes list
                /// </summary>
                public int LastScene { get; protected set; }
                /// <summary>
                /// Integer Identifier for Current Message element in the DialogObject's Messages list
                /// </summary>
                public int CurrentMessage { get; protected set; }
                /// <summary>
                /// Index of the last message in the Messages list
                /// </summary>
                public int LastMessage { get; protected set; }
                /// <summary>
                /// The current actual string being drawn
                /// </summary>
                public string MessageDraw { get; protected set; }
                /// <summary>
                /// Speed at which letters appear
                /// </summary>
                public double Increase { get; protected set; }
                /// <summary>
                /// Number of characters in the current dialog MessageDraw
                /// </summary>
                public int Characters { get; protected set; }
                /// <summary>
                /// Multiplier used to accelerate type-writing effect when action key is pressed
                /// </summary>
                public int Hold { get; protected set; }
                /// <summary>
                /// Total Message Length in characters. Compared against the Characters variable
                /// to determine when entire message is on-screen
                /// </summary>
                public int MessageLength { get; protected set; }
                /// <summary>
                /// Texture drawn behind the Dialog Messages
                /// </summary>
                public Texture2D MessageBox { get; protected set; }
                /// <summary>
                /// Destruction Flag for Dialog Object; MainGame will remove this Dialog Object if this is flagged true
                /// </summary>
                public bool DestroyMe { get; protected set; }
                /// <summary>
                /// Position of the Dialog Object, used to place the MessageBox
                /// and Message string
                /// </summary>
                public Vector2 Position { get; protected set; }
                /// <summary>
                /// Position of the MessageBox texture; should be slightly left and up from the DialogObject Position
                /// </summary>
                public Vector2 MessageBoxPosition { get; protected set; }
                /// <summary>
                /// Spritefont used to Draw the Message string
                /// </summary>
                public SpriteFont MessageFont { get; protected set; }
                /// <summary>
                /// Boolean used to determine if the MainGame has noticed an ActionKey press
                /// </summary>
                public bool ActionKeyPressed { get; protected set; }
                /// <summary>
                /// Boolean that triggers when there are no more characters to display from the current message
                /// </summary>
                public bool FullMessageDisplayed { get; protected set; }
                /// <summary>
                /// The width/height of the game screen. Used to position the DialogObject and MessageBox
                /// </summary>
                private Vector2 DisplaySize { get; set; }
                /// <summary>
                /// Maximum Length (in pixels) that a single line of characters or string
                /// can be before WrapText kicks in
                /// </summary>
                protected float MaxLineWidth { get; set; }
                /// <summary>
                /// String used to fill the 'speaking character' portion of the
                /// MessageBox texture. Signafies who is currently using the
                /// DialogObject
                /// </summary>
                public string DialogCharacter { get; protected set; }
                /// <summary>
                /// ChoiceDialog object to display a set of choices at the end
                /// of the Messages list
                /// </summary>
                public ChoiceDialog ChoiceDialog { get; protected set; }
                /// <summary>
                /// Index of the Selected Choice; Can be used any number of ways,
                /// but general for Dialog tree options
                /// </summary>
                public int SelectedChoice { get; protected set; }
                public PlayerIndex MyPlayer { get; protected set; }
                private bool NextPressed { get; set; }
                private bool PreviousPressed { get; set; }
                private bool CommandRelease { get; set; }
                private float InputTimer { get; set; }
                private float InputTimerReset { get; set; }

                /// <summary>
                /// Create a Dialog Object with an identifier, message color,
                /// and any number of message strings
                /// </summary>
                /// <param name="id">Identifier Integer; Used for Reference</param>
                /// <param name="message_color">Color to Draw the Text in</param>
                /// <param name="messages">Any number of string messages to display to screen, separated by comma</param>
                public DialogObject(int id, string character, Color message_color, SpriteFont message_font, Texture2D messagebox_texture, Vector2 screensize, PlayerIndex myplayer, params string[] messages) {
                    ID = id;
                    MessageColor = message_color;
                    Messages = new List<string>();
                    Scenes = null;
                    SceneDriven = false;
                    SceneImage = null;
                    MessageFont = message_font;
                    MessageBox = messagebox_texture;
                    CurrentScene = 0;
                    LastScene = 0;
                    DisplaySize = screensize;
                    DialogCharacter = character;
                    MyPlayer = myplayer;

                    for (int i = 0; i < messages.Length; i++) {
                        Messages.Add(messages[i]);
                    }

                    Initialize();
                }

                /// <summary>
                /// Creates a Dialog Object with an identifier, message color, and
                /// preset list of message strings
                /// </summary>
                /// <param name="id">Integer Identifier</param>
                /// <param name="message_color">Color to Draw the Text in</param>
                /// <param name="messages">List of Message Strings to Transfer to the Dialog Object</param>
                public DialogObject(int id, string character, Color message_color, SpriteFont message_font, Texture2D messagebox_texture, Vector2 screensize, PlayerIndex myplayer, List<string> messages) {
                    ID = id;
                    MessageColor = message_color;
                    Messages = new List<string>(messages);
                    SceneDriven = false;
                    SceneImage = null;
                    Scenes = null;
                    MessageFont = message_font;
                    MessageBox = messagebox_texture;
                    CurrentScene = 0;
                    LastScene = 0;
                    DisplaySize = screensize;
                    DialogCharacter = character;
                    MyPlayer = myplayer;

                    Initialize();
                }

                /// <summary>
                /// Creates a copy of the DialogObject
                /// </summary>
                /// <param name="dialog">DialogObject to Copy</param>
                public DialogObject(DialogObject dialog, PlayerIndex myplayer) {
                    ID = dialog.ID;
                    MessageColor = dialog.MessageColor;
                    Messages = new List<string>(dialog.Messages);
                    SceneDriven = dialog.SceneDriven;
                    SceneImage = dialog.SceneImage;
                    Scenes = new List<Scene>(dialog.Scenes);
                    MessageFont = dialog.MessageFont;
                    MessageBox = dialog.MessageBox;
                    CurrentScene = dialog.CurrentScene;
                    LastScene = dialog.LastScene;
                    DisplaySize = dialog.DisplaySize;
                    DialogCharacter = dialog.DialogCharacter;
                    MyPlayer = myplayer;

                    Initialize();
                }

                /// <summary>
                /// Creates a Dialog Object with an identifier, message color,
                /// and list of Scene objects used to create a cut-scene effect.
                /// Each scene must already be built and placed into a list.
                /// </summary>
                /// <param name="id"></param>
                /// <param name="message_color"></param>
                /// <param name="scenes"></param>
                public DialogObject(int id, string character, Color message_color, SpriteFont message_font, Texture2D messagebox_texture, Vector2 screensize, PlayerIndex myplayer, List<Scene> scenes) {
                    ID = id;
                    MessageColor = message_color;
                    Scenes = new List<Scene>(scenes);
                    Messages = new List<string>(Scenes.ElementAt(0).Messages);
                    SceneDriven = true;
                    SceneImage = Scenes.ElementAt(0).SceneImage;
                    MessageFont = message_font;
                    MessageBox = messagebox_texture;
                    CurrentScene = 0;
                    LastScene = scenes.Count - 1;
                    DisplaySize = screensize;
                    DialogCharacter = character;
                    MyPlayer = myplayer;

                    Initialize();
                }

                /// <summary>
                /// Sets the standard variables for dialog
                /// </summary>
                private void Initialize() {
                    CurrentMessage = 0;
                    LastMessage = Messages.Count - 1;
                    MessageDraw = "";
                    Increase = 0.5;
                    Characters = 0;
                    Hold = 0;
                    MessageLength = Messages.ElementAt(CurrentMessage).Length;
                    DestroyMe = false;
                    ActionKeyPressed = false;
                    NextPressed = false;
                    PreviousPressed = false;
                    CommandRelease = true;
                    MaxLineWidth = (MessageBox.Width - 16);
                    InputTimerReset = 0.10f;
                    InputTimer = InputTimerReset;

                    float mbx, mby, msgx, msgy;

                    mbx = (DisplaySize.X / 2) - (MessageBox.Width / 2);
                    mby = DisplaySize.Y - (MessageBox.Height + 12);

                    msgx = mbx + 12;
                    msgy = mby + 28;

                    MessageBoxPosition = new Vector2(mbx, mby);
                    Position = new Vector2(msgx, msgy);
                }

                /// <summary>
                /// Enables the Hold variable for slower message display
                /// </summary>
                public void EnableHold() {
                    Hold = 1;
                }

                /// <summary>
                /// Disables the Hold variable for
                /// quicker message display
                /// </summary>
                public void DisableHold() {
                    Hold = 0;
                }

                /// <summary>
                /// Sets the ActionKey to Pressed;
                /// Allows Dialog to move to the next
                /// message or close.
                /// </summary>
                /// <returns></returns>
                public void ContinueKey() {
                    ActionKeyPressed = true;
                    EnableHold();
                }

                /// <summary>
                /// Adds a ChoiceDialog to this DialogObject, which will
                /// display after the last message is fully written.
                /// </summary>
                /// <param name="choicedialog">A pre-defined ChoiceDialog to attach to this DialogObject</param>
                public void AttachChoice(ChoiceDialog choicedialog) {
                    ChoiceDialog = choicedialog;
                    SelectedChoice = 0;
                }

                /// <summary>
                /// Adds a ChoiceDialog to this DialogObject, which will
                /// display after the last message is fully written.
                /// </summary>
                /// <param name="choices">A list of string options to include in the ChoiceDialog</param>
                public void CreateChoice(Texture2D dialog_texture, Texture2D selection_texture, params string[] choices) {
                    ChoiceDialog choicedialog = new ChoiceDialog(this, dialog_texture, selection_texture, choices);
                    SelectedChoice = 0;
                }

                /// <summary>
                /// Adds a ChoiceDialog to this DialogObject, which will
                /// display after the last message is fully written.
                /// </summary>
                /// <param name="choices">A pre-defined list of choices to include in the ChoiceDialog</param>
                public void CreateChoice(Texture2D dialog_texture, Texture2D selection_texture, List<Choice> choices) {
                    ChoiceDialog choicedialog = new ChoiceDialog(this, dialog_texture, selection_texture, choices);
                    SelectedChoice = 0;
                }

                /// <summary>
                /// Checks for Input important to the dialog box
                /// </summary>
                /// <param name="gpstate"></param>
                /// <param name="keystate"></param>
                public void ProcessInput(GamePadState gpstate, KeyboardState keystate) {
                    if (InputTimer <= 0) {
                        if ((keystate.IsKeyDown(Keys.Space) == true)) {
                            ActionKeyPressed = true;
                        }

                        if ((keystate.IsKeyDown(Keys.Up) == true)) {
                            PreviousPressed = true;
                        }

                        if ((keystate.IsKeyDown(Keys.Down) == true)) {
                            NextPressed = true;
                        }
                    }
                }

                /// <summary>
                /// Runs Update Method for Dialog object; Calculates how much of the current
                /// Message string is visible
                /// </summary>
                /// <param name="gameTime"></param>
                public virtual void Update(GameTime gameTime) {
                    GamePadState gpstate = GamePad.GetState(MyPlayer);
                    KeyboardState keystate = Keyboard.GetState();

                    ProcessInput(gpstate, keystate);

                    if (Characters < MessageLength) {
                        Characters = Characters + (int)Math.Ceiling(((Increase * (1 + Hold)) * gameTime.ElapsedGameTime.TotalSeconds));
                        MessageDraw = Messages.ElementAt(CurrentMessage).Substring(0, Characters);
                        MessageDraw = DialogController.WrapText(MessageFont, MessageDraw, MaxLineWidth);
                        /*
                         * Check Audio Here, Play Audio SFX for
                         * Dialog if it is not currently playing
                         */
                    }
                    else {
                        if (FullMessageDisplayed == false) {
                            FullMessageDisplayed = true;
                        }

                        if (InputTimer > 0) {
                            InputTimer = InputTimer - (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }

                        // Check if this is the last message so we can
                        // do closing/deletion or ChoiceDialog processing
                        if (CurrentMessage == LastMessage) {
                            if ((SceneDriven == true) && (CurrentScene < LastScene)) {
                                if (ActionKeyPressed == true) {
                                    // Move to the next scene
                                    CurrentScene = CurrentScene + 1;
                                    CurrentMessage = 0;
                                    Scene scene = Scenes.ElementAt(CurrentScene);
                                    Messages = new List<string>(scene.Messages);
                                    SceneImage = scene.SceneImage;
                                    ActionKeyPressed = false;
                                    InputTimer = InputTimerReset;
                                }
                            }
                            else {
                                if (ChoiceDialog != null) {
                                    if (NextPressed == true) {
                                        SelectedChoice = ChoiceDialog.SelectNext();
                                        NextPressed = false;
                                        InputTimer = InputTimerReset;
                                    }
                                    if (PreviousPressed == true) {
                                        SelectedChoice = ChoiceDialog.SelectPrevious();
                                        PreviousPressed = false;
                                        InputTimer = InputTimerReset;
                                    }
                                    if (ActionKeyPressed == true) {
                                        SelectedChoice = ChoiceDialog.SelectChoice();
                                        ActionKeyPressed = false;
                                        InputTimer = InputTimerReset;
                                        DestroyMe = true;
                                    }
                                }
                                else {
                                    if (ActionKeyPressed == true) {
                                        DestroyMe = true;
                                    }
                                }
                            }
                        }

                        // Do processing not related to end-of-dialog
                        // options
                        else {
                            if (ActionKeyPressed == true) {
                                CurrentMessage = CurrentMessage + 1;
                                MessageLength = Messages.ElementAt(CurrentMessage).Length;
                                Characters = 0;
                                MessageDraw = "";
                                FullMessageDisplayed = false;
                                ActionKeyPressed = false;
                                InputTimer = InputTimerReset;
                            }
                        }
                    }
                }

                /// <summary>
                /// Draws the MessageBox texture, Current Message string, and
                /// any other Dialog options as necessary
                /// </summary>
                /// <param name="spriteBatch"></param>
                public virtual void Draw(SpriteBatch spriteBatch) {
                    // If the dialog has a scene image, draw it first
                    if (SceneDriven == true) {
                        spriteBatch.Draw(SceneImage, new Vector2(0, 0), Color.White);
                    }

                    // If this Dialog has a choice AND the full dialog is displayed
                    if ((ChoiceDialog != null) &&
                        (CurrentMessage == LastMessage) &&
                        (FullMessageDisplayed == true)) {

                        ChoiceDialog.Draw(spriteBatch);
                    }

                    spriteBatch.Draw(MessageBox, MessageBoxPosition, Color.White);
                    spriteBatch.DrawString(MessageFont, DialogCharacter, new Vector2(MessageBoxPosition.X + 36, MessageBoxPosition.Y + 6), Color.White);
                    spriteBatch.DrawString(MessageFont, MessageDraw, Position, MessageColor);

                    //string debug1 = "MessageBox X,Y: " + MessageBoxPosition.X.ToString() + "," + MessageBoxPosition.Y.ToString();
                    //string debug2 = "DialogObject X,Y: " + Position.X.ToString() + "," + Position.Y.ToString();

                    //spriteBatch.DrawString(MessageFont, debug1, new Vector2(16, 16), Color.Red);
                    //spriteBatch.DrawString(MessageFont, debug2, new Vector2(16, 32), Color.Red);
                }
            }
            public class DialogTree {
                public int ID { get; protected set; }
                public bool DestroyMe { get; protected set; }
                private int CurrentAccessChoice { get; set; }
                public List<DialogBranch> Branches { get; protected set; }
                public List<DialogObject> Dialogs { get; protected set; }
                public DialogBranch CurrentBranch { get; protected set; }
                public DialogObject CurrentDialogObject { get; protected set; }
                public int CurrentDialog { get; protected set; }

                public float KeyTimer { get; protected set; }
                public float KeyTimerReset { get; protected set; }

                public DialogTree(int id, List<DialogBranch> branches) {
                    ID = id;
                    Branches = new List<DialogBranch>(branches);
                }

                private void Initialize() {
                    Dialogs = new List<DialogObject>();
                    DestroyMe = false;
                    CurrentAccessChoice = 0;
                    CurrentBranch = GetBranch(CurrentAccessChoice);
                    CurrentDialog = 0;
                    CurrentDialogObject = CurrentBranch.Dialogs.ElementAt(0);
                    KeyTimer = 0;
                    KeyTimerReset = 0.10f;
                }

                public DialogBranch GetBranch(int access) {
                    foreach (DialogBranch branch in Branches) {
                        if (branch.AccessChoice == access) {
                            return branch;
                        }
                    }

                    return null;
                }

                public void Complete() {
                    DestroyMe = true;
                }
            }
            public class Scene {
                public int ID { get; protected set; }
                public Texture2D SceneImage { get; protected set; }
                public List<string> Messages { get; protected set; }

                public Scene(int id, Texture2D scene_image, List<string> messages) {
                    ID = id;
                    SceneImage = scene_image;
                    Messages = new List<string>(messages);
                }

                public Scene(int id, Texture2D scene_image, params string[] messages) {
                    ID = id;
                    SceneImage = scene_image;
                    Messages = new List<string>();

                    for (int i = 0; i < messages.Length; i++) {
                        Messages.Add(messages[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Derivative Library of Controls and Classes
        /// used to create and operate Splashscreens,
        /// Titlescreens, and Menus
        /// </summary>
        namespace ScreenEngine {
            
            public class ScreenController {
                /* Enums */
                public enum ScrollType {
                    Right,
                    Left,
                    Up,
                    Down,
                    None
                }
                
                /* Members */
                public readonly SpriteBatch SC_SpriteBatch;
                public readonly ContentManager SC_Contentmanager;
                private Action onGameExit;

                private readonly List<IScreen> GameScreens = new List<IScreen>();

                public ScreenController(SpriteBatch spriteBatch, ContentManager content) {
                    SC_SpriteBatch = spriteBatch;
                    SC_Contentmanager = content;
                }

                private bool IsScreenListEmpty {
                    get {
                        return GameScreens.Count <= 0;
                    }
                }
                private IScreen GetCurrentScreen() {
                    return GameScreens.ElementAt(GameScreens.Count - 1);
                }
                private void RemoveCurrentScreen() {
                    IScreen screen = (IScreen)GetCurrentScreen();

                    screen.Dispose();

                    GameScreens.Remove(screen);
                }
                private void RemoveAllScreens() {
                    while (!IsScreenListEmpty) {
                        RemoveCurrentScreen();
                    }
                }
                public void ChangeBetweenScreens() {
                    
                }
                public void ChangeScreen(IScreen screen) {
                    RemoveAllScreens();

                    GameScreens.Add(screen);

                    screen.Initialize();
                }
                public void PushScreen(IScreen screen) {
                    if (!IsScreenListEmpty) {
                        IScreen curScreen = (IScreen)GetCurrentScreen();
                        curScreen.Pause();
                    }

                    GameScreens.Add(screen);

                    screen.Initialize();
                }
                public void PopScreen() {
                    if (!IsScreenListEmpty) {
                        RemoveCurrentScreen();
                    }

                    if (!IsScreenListEmpty) {
                        IScreen screen = GetCurrentScreen();
                        screen.Resume();
                    }
                }
                public void Update(GameTime gameTime) {
                    if (!IsScreenListEmpty) {
                        IScreen screen = GetCurrentScreen();

                        if(screen.IsScreenPaused() == false) {
                            screen.Update(gameTime);
                        }
                    }
                }
                public void Draw(GameTime gameTime) {
                    if (!IsScreenListEmpty) {
                        IScreen screen = GetCurrentScreen();

                        SC_SpriteBatch.Begin();

                        if (screen.IsScreenPaused() == false) {
                            screen.Draw(SC_SpriteBatch);
                        }

                        SC_SpriteBatch.End();
                    }
                }
                public void HandleInput(GameTime gameTime) {
                    if (!IsScreenListEmpty) {
                        IScreen screen = GetCurrentScreen();

                        if(screen.IsScreenPaused() == false) {
                            screen.HandleInput(gameTime);
                        }
                    }
                }
                public void Exit() {
                    if (onGameExit != null) {
                        onGameExit();
                    }
                }
                public void Dispose() {
                    RemoveAllScreens();
                }
                public static Vector2 GetScrollVector(ScrollType scroll, float scroll_rate) {
                    Vector2 scroll_vector = new Vector2(0);

                    switch (scroll) {
                        case ScrollType.Right: {
                                scroll_vector = new Vector2(scroll_rate, 0);
                                break;
                            }
                        case ScrollType.Left: {
                                scroll_vector = new Vector2(-scroll_rate, 0);
                                break;
                            }
                        case ScrollType.Up: {
                                scroll_vector = new Vector2(0, -scroll_rate);
                                break;
                            }
                        case ScrollType.Down: {
                                scroll_vector = new Vector2(0, scroll_rate);
                                break;
                            }
                    }

                    return scroll_vector;
                }
                public event Action OnGameExit {
                    add { onGameExit += value; }
                    remove { onGameExit -= value; }
                }
            }

            public interface IScreen : IDisposable {
                void Initialize();

                void Pause();
                void Resume();
                bool IsScreenPaused();

                void HandleInput(GameTime gameTime);
                void Update(GameTime gameTime);
                void Draw(SpriteBatch spriteBatch);

                void AddElement(IScreenElement element);
            }
            public class Screen : IScreen {
                public int ID { get; protected set; }
                public Vector2 ViewSize { get; protected set; }
                public bool IsPaused { get; protected set; }
                public List<IScreenElement> Elements { get; protected set; }

                public Screen(int id, Vector2 size) {
                    ID = id;
                    ViewSize = size;

                    Resume();
                    Elements = new List<IScreenElement>();
                }

                public void Pause() {
                    IsPaused = true;
                }

                public void Resume() {
                    IsPaused = false;
                }

                public bool IsScreenPaused() {
                    return IsPaused;
                }

                public void AddElement(IScreenElement element) {
                    Elements.Add(element);
                }

                public void Initialize() {
                    foreach (IScreenElement element in Elements) {
                        element.Initialize();
                    }
                }

                public void Update(GameTime gameTime) {
                    foreach (IScreenElement element in Elements) {
                        element.Update(gameTime);
                    }
                }

                public void HandleInput(GameTime gameTime) {
                    
                }

                public void Draw(SpriteBatch spriteBatch) {
                    foreach (IScreenElement element in Elements) {
                        element.Draw(spriteBatch);
                    }
                }

                public void Dispose() {
                    for (int i = Elements.Count - 1; i >= 0; i--) {
                        Elements.RemoveAt(i);
                    }
                }
            }

            public interface IScreenElement : IDisposable {
                void Initialize();
                void Draw(SpriteBatch spriteBatch);
                void Update(GameTime gameTime);
            }
            public abstract class ScreenElement {
                public int ID { get; protected set; }
                public Vector2 Position { get; protected set; }
                public bool Visible { get; protected set; }
                protected Vector2 EndPosition { get; set; }
                protected bool IsStationary { get; set; }
                protected float Timer { get; set; }
                protected float TimerReset { get; set; }
                protected bool Initialized { get; set; }

                protected void ProcessTimers(GameTime gameTime) {
                    if(Timer > 0) {
                        Timer = Timer - (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }

                /// <summary>
                /// Sets Element Visible to True
                /// </summary>
                public void Show() {
                    Visible = true;
                }

                /// <summary>
                /// Sets Element Visible to False
                /// </summary>
                public void Hide() {
                    Visible = false;
                }

                /// <summary>
                /// Resets Element Timer to predefined time (in seconds)
                /// </summary>
                protected void ResetTimer() {
                    Timer = TimerReset;
                }
            }
            public class TextElement : ScreenElement, IScreenElement {
                public string Text { get; protected set; }
                private SpriteFont Font { get; set; }
                private Vector2 DrawPosition { get; set; }
                private int TextAlignment { get; set; }

                /// <summary>
                /// Creates a Stationary TextElement at specified
                /// Position
                /// </summary>
                /// <param name="id">Integer Identifier</param>
                /// <param name="text">String to Display</param>
                /// <param name="position">X,Y to Place the Element</param>
                public TextElement(int id, string text, int text_alignment, SpriteFont font, Vector2 position) {
                    ID = id;
                    Text = text;
                    Position = position;
                    TextAlignment = text_alignment;
                    Font = font;

                    Hide();
                    IsStationary = true;
                    EndPosition = new Vector2(0);
                    Initialized = false;
                }

                /// <summary>
                /// Creates a non-Stationary TextElement that starts at specified
                /// Position and stops at second specified position over a period of
                /// seconds also specified.
                /// </summary>
                /// <param name="id">Integer Identifier</param>
                /// <param name="text">String to Display</param>
                /// <param name="text_alignment">Sets Text-Alignment Option; 0 for Left, 1 for Centered, 2 for Right</param>
                /// <param name="font">SpriteFont used to Draw the TextElement</param>
                /// <param name="startpos">X,Y to Place the Element at Start</param>
                /// <param name="endpos">X,Y to End Element movement</param>
                /// <param name="move_timer">Amount of Time to complete movement from startpos to endpos (in seconds)</param>
                public TextElement(int id, string text, int text_alignment, SpriteFont font, Vector2 startpos, Vector2 endpos, float move_timer) {
                    ID = id;
                    Text = text;
                    TextAlignment = text_alignment;
                    Font = font;
                    Position = startpos;
                    EndPosition = endpos;
                    TimerReset = move_timer;

                    Hide();
                    IsStationary = false;
                    Initialized = false;
                }

                public void Initialize() {
                    Show();
                    SetDrawPosition();
                    Initialized = true;
                }

                public void Update(GameTime gameTime) {
                    ProcessTimers(gameTime);

                }

                public void Draw(SpriteBatch spriteBatch) {
                    spriteBatch.DrawString(Font, Text, DrawPosition, Color.White);
                }

                public void Dispose() {

                }

                /// <summary>
                /// Sets DrawPosition based on Text-Alignment Option
                /// </summary>
                private void SetDrawPosition() {
                    switch (TextAlignment) {
                        case 0: {   // Left Alignment
                                float yy;

                                yy = Position.Y - Font.MeasureString(Text).Y / 2;

                                DrawPosition = new Vector2(Position.X, yy);

                                break;
                            }
                        case 1: {   // Center Alignment
                                float xx, yy;

                                xx = Position.X - Font.MeasureString(Text).X / 2;
                                yy = Position.Y - Font.MeasureString(Text).Y / 2;

                                DrawPosition = new Vector2(xx, yy);

                                break;
                            }
                        case 2: {   // Right Alignment
                                float xx, yy;

                                xx = Position.X - Font.MeasureString(Text).X;
                                yy = Position.Y - Font.MeasureString(Text).Y / 2;

                                DrawPosition = new Vector2(xx, yy);

                                break;
                            }
                    }
                }
            }
            public class TextureElement : ScreenElement, IScreenElement {
                private Texture2D Texture { get; set; }
                private ScreenController.ScrollType Scroll { get; set; }
                private float ScrollRate { get; set; }
                private Vector2 Velocity { get; set; }
                private Vector2 ScrollbackPosition { get; set; }
                private Vector2 DisplaySize { get; set; }
                private Rectangle DisplayRectangle { get; set; }
                private Rectangle ScrollbackRectangle { get; set; }

                public TextureElement(int id, Texture2D texture, Vector2 position) {
                    ID = id;
                    Texture = texture;
                    Position = position;
                    Scroll = ScreenController.ScrollType.None;
                    ScrollRate = 0f;

                    Initialize();
                }

                public TextureElement(int id, Texture2D texture, Vector2 position, ScreenController.ScrollType scroll, float scroll_speed, Vector2 displaysize) {
                    ID = id;
                    Texture = texture;
                    Position = position;
                    Scroll = scroll;
                    ScrollRate = scroll_speed;
                    DisplaySize = displaysize;

                    Initialize();
                }

                /* Base Methods */
                public void Initialize() {
                    Velocity = new Vector2(0);
                    ScrollbackPosition = SetScrollbackVector();
                    UpdateRectangles();
                }

                public void Update(GameTime gameTime) {
                    ProcessTimers(gameTime);

                    Velocity = ScreenController.GetScrollVector(Scroll, ScrollRate);
                    Position = Position + Velocity;
                    Velocity = new Vector2(0);

                    if(Scroll != ScreenController.ScrollType.None) {
                        switch (Scroll) {
                            case ScreenController.ScrollType.Left: {
                                    if (Position.X <= -DisplaySize.X) {
                                        Position = new Vector2(0);
                                    }
                                    break;
                                }
                            case ScreenController.ScrollType.Right: {
                                    if (Position.X >= DisplaySize.X) {
                                        Position = new Vector2(0);
                                    }
                                    break;
                                }
                            case ScreenController.ScrollType.Up: {
                                    if (Position.Y <= -DisplaySize.Y) {
                                        Position = new Vector2(0);
                                    }
                                    break;
                                }
                            case ScreenController.ScrollType.Down: {
                                    if (Position.Y >= DisplaySize.Y) {
                                        Position = new Vector2(0);
                                    }
                                    break;
                                }
                        }

                        ScrollbackPosition = SetScrollbackVector();

                        UpdateRectangles();
                    }
                }

                public void Draw(SpriteBatch spriteBatch) {
                    spriteBatch.Draw(Texture, DisplayRectangle, Color.White);
                    spriteBatch.Draw(Texture, ScrollbackRectangle, Color.White);
                    //spriteBatch.Draw(Texture, Position, Color.White);
                    //spriteBatch.Draw(Texture, ScrollbackPosition, Color.White);
                }

                public void Dispose() {

                }

                /* Methods */
                private Vector2 SetScrollbackVector() {
                    Vector2 scrollback = new Vector2(0);

                    switch (Scroll) {
                        case ScreenController.ScrollType.Left: {
                                scrollback = new Vector2(DisplaySize.X, 0);
                                break;
                            }
                        case ScreenController.ScrollType.Right: {
                                scrollback = new Vector2(-DisplaySize.X, 0);
                                break;
                            }
                        case ScreenController.ScrollType.Up: {
                                scrollback = new Vector2(0, DisplaySize.Y);
                                break;
                            }
                        case ScreenController.ScrollType.Down: {
                                scrollback = new Vector2(0, -DisplaySize.Y);
                                break;
                            }
                    }

                    scrollback = Position + scrollback;

                    return scrollback;
                }

                private void UpdateRectangles() {
                    DisplayRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)DisplaySize.X, (int)DisplaySize.Y);
                    ScrollbackRectangle = new Rectangle((int)ScrollbackPosition.X, (int)ScrollbackPosition.Y, (int)DisplaySize.X, (int)DisplaySize.Y);
                }
            }
            public class SoundElement : ScreenElement, IScreenElement {


                public void Initialize() {
                }

                public void Update(GameTime gameTime) {
                    ProcessTimers(gameTime);

                }

                public void Draw(SpriteBatch spriteBatch) {

                }

                public void Dispose() {

                }
            }
            public class MusicElement : ScreenElement, IScreenElement {


                public void Initialize() {
                }

                public void Update(GameTime gameTime) {
                    ProcessTimers(gameTime);

                }

                public void Draw(SpriteBatch spriteBatch) {

                }

                public void Dispose() {

                }
            }
            public class EntityElement : ScreenElement, IScreenElement {
                public IEntity Element { get; protected set; }

                public EntityElement(IEntity entity) {
                    Element = entity;
                }

                public void Initialize() {
                    Element.Initialize();
                }

                public void Update(GameTime gameTime) {
                    Element.Update(gameTime);
                }

                public void Draw(SpriteBatch spriteBatch) {
                    Element.Draw(spriteBatch);
                }

                public void Dispose() {
                    Element.Dispose();
                }
            }
            public class MenuElement : ScreenElement, IScreenElement {
                protected List<MenuItem> Items { get; private set; }
                private int CurrentItem { get; set; }
                private int LastItem { get; set; }

                public void Initialize() {
                    CurrentItem = 0;
                    LastItem = Items.Count - 1;
                }

                public void Update(GameTime gameTime) {
                    ProcessTimers(gameTime);

                }

                public void Draw(SpriteBatch spriteBatch) {

                }

                public void Dispose() {
                    for(int i = Items.Count - 1; i >= 0; i--) {
                        Items.RemoveAt(i);
                    }
                }

                public void NextItem() {
                    MenuItem item = Items.ElementAt(CurrentItem);
                    item.Deselect();

                    if(CurrentItem < LastItem) {
                        CurrentItem = CurrentItem + 1;
                    }
                    else {
                        CurrentItem = 0;
                    }

                    item = Items.ElementAt(CurrentItem);
                    item.Select();
                }

                public void PreviousItem() {
                    MenuItem item = Items.ElementAt(CurrentItem);
                    item.Deselect();

                    if (CurrentItem > 0) {
                        CurrentItem = CurrentItem - 1;
                    }
                    else {
                        CurrentItem = LastItem;
                    }

                    item = Items.ElementAt(CurrentItem);
                    item.Select();
                }
            }
            public class MenuItem {
                private int ID { get; set; }
                private string Text { get; set; }
                private SpriteFont Font { get; set; }
                private Color Color { get; set; }
                private bool IsSelected { get; set; }
                private Vector2 Position { get; set; }
                private Vector2 DrawPosition { get; set; }

                public MenuItem(int id, string text, SpriteFont font, Color color, Vector2 position) {
                    ID = id;
                    Text = text;
                    Font = font;
                    Position = position;
                    Color = color;
                    IsSelected = false;

                    SetDrawPosition();
                }

                public void Deselect() {
                    IsSelected = false;
                }

                public void Select() {
                    IsSelected = true;
                }

                public void SetDrawPosition() {
                    int xx = (int)Position.X;
                    int yy = (int)Position.Y;

                    xx = xx - (int)(Font.MeasureString(Text).X / 2);
                    yy = yy - (int)(Font.MeasureString(Text).Y / 2);

                    DrawPosition = new Vector2(xx, yy);
                }

                public void Draw(SpriteBatch spriteBatch) {
                    spriteBatch.DrawString(Font, Text, DrawPosition, Color);
                }
            }
        }
    }
}
