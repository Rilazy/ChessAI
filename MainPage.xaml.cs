﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Svg;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
// (I wrote most of the code here, but there are templates that generate the boilerplate code for me)

namespace ChessAI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page //A sealed class means nothing can inherit from this class, and partial means it can be defined in several parts over multiple files if I so choose.
    {
        public CanvasSvgDocument Board; //These are all declaring the variables that will hold the images for the pieces and the board as SVG's that I have drawn.
        public CanvasSvgDocument PawnBlack;
        public CanvasSvgDocument PawnWhite;
        public CanvasSvgDocument KnightBlack;
        public CanvasSvgDocument KnightWhite;
        public CanvasSvgDocument BishopBlack;
        public CanvasSvgDocument BishopWhite;
        public CanvasSvgDocument RookBlack;
        public CanvasSvgDocument RookWhite;
        public CanvasSvgDocument QueenBlack;
        public CanvasSvgDocument QueenWhite;
        public CanvasSvgDocument KingBlack;
        public CanvasSvgDocument KingWhite;
        public CanvasSvgDocument PlaceholderBlack;
        public CanvasSvgDocument PlaceholderWhite;

        private Color MoveHighlight;
        private Pawn pawn;
        
        private Size pieceSize;

        public static GameState game = new GameState();

        private RenderTranslator translator;



        public MainPage() //This is auto-generated boilerplate code that interacts with Win2D (the graphics API I'm using). All it does is tell Win2D the window should exist.
        {
            this.InitializeComponent();

            canvas.PointerPressed += new PointerEventHandler(input_MouseClicked); //This tells the canvas which event it should raise (function it should call) when it is clicked
            
        }

        
        
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args) //This is where I put the code that actually draws anything on screen.
        {
            // The actual renderer (currently Win2D)
            
            args.DrawingSession.DrawSvg(Board, sender.Size); //It works a lot like processing actually, but it's in C# and hardware accelerated (it runs on the GPU not the CPU), which is ideal and why I chose it
            
            foreach (Tuple<Position, byte> piece in game.RenderInterface()) //Iterate over each of the Tuples returned by game.RenderInterface()
            {
                args.DrawingSession.DrawSvg(translator.map[piece.Item2], pieceSize, (float)(piece.Item1.Column * pieceSize.Width), (float)(piece.Item1.Row * pieceSize.Height));
                //The function call    Have the translator find the svg  The size    Calculate on-screen location based on board position by multiplying board positions by the width of each tile
            }

            foreach (Position move in game.Moves) {
                args.DrawingSession.DrawCircle((float)(move.Column * pieceSize.Width + pieceSize.Width / 2), (float)(move.Row * pieceSize.Height + pieceSize.Height / 2), (float)(pieceSize.Height / 4), MoveHighlight);
            }
            
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) //This is more boilerplate code (though not fully autogenerated). It runs when the window is closed and runs the code to properly dispose of the memory it used
        {
            this.canvas.RemoveFromVisualTree();
            this.canvas = null;
        }

        private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
            //Yet more boilerplate code, this runs before canvas_Draw and is meant to be used to load any assets that need to be loaded from files
        {
            Board = LoadAsset(sender, "Chess Board"); //These are all basically identical; they set the value of each variable to whatever LoadAsset returns given the file name.
            PawnBlack = LoadAsset(sender, "PawnBlack");
            PawnWhite = LoadAsset(sender, "PawnWhite");
            KnightBlack = LoadAsset(sender, "KnightBlack");
            KnightWhite = LoadAsset(sender, "KnightWhite");
            BishopBlack = LoadAsset(sender, "BishopBlack");
            BishopWhite = LoadAsset(sender, "BishopWhite");
            RookBlack = LoadAsset(sender, "RookBlack");
            RookWhite = LoadAsset(sender, "RookWhite");
            QueenBlack = LoadAsset(sender, "QueenBlack");
            QueenWhite = LoadAsset(sender, "QueenWhite");
            KingBlack = LoadAsset(sender, "KingBlack");
            KingWhite = LoadAsset(sender, "KingWhite");

            PlaceholderBlack = CanvasSvgDocument.LoadFromXml(sender, PawnBlack.GetXml().Replace("opacity=\"1\"", "opacity=\"0.5\""));
            PlaceholderWhite = CanvasSvgDocument.LoadFromXml(sender, PawnWhite.GetXml().Replace("opacity=\"1\"", "opacity=\"0.5\""));

            translator = new RenderTranslator(PawnBlack, PawnWhite, KnightBlack, KnightWhite, BishopBlack, BishopWhite, RookBlack, RookWhite, QueenBlack, QueenWhite, KingBlack, KingWhite, PlaceholderBlack, PlaceholderWhite);
            pieceSize = new Size(sender.Size.Width / 16, sender.Size.Height / 16);

            pawn = new Pawn(new Position(3, 3), true);
            MoveHighlight = new Color();
            MoveHighlight.A = 100;
            MoveHighlight.R = 255;
            MoveHighlight.G = 255;
            MoveHighlight.B = 255;

        }

        public static CanvasSvgDocument LoadAsset(CanvasControl sender, string fileName) //I wrote this one to clean up the previously very messy code in canvas_CreatResources; it deals with the file handling so I don't need to worry about that for each individual asset
        {
            var file = File.ReadAllText($"Assets/{fileName}.svg"); //File.ReadAllText simply reads the whole file as a string, the $ strings work exactly like python f-strings, and Assets/ is the path where the assets are kept
            return CanvasSvgDocument.LoadFromXml(sender, file); //CanvasSvgDocument.LoadFromXml takes in a string which is a valid Svg file and loads it into whatever form Win2D needs
        }

        

        
    }
}
