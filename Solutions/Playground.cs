
using FoundryBlazor.Shape;
using Radzen;

using Microsoft.JSInterop;
using FoundryBlazor.Dialogs;
using Microsoft.AspNetCore.Components;
using FoundryBlazor.Canvas;

namespace FoundryBlazor.Solutions;


public class Playground
{
    private IDrawing Drawing { get; set; }
    private DialogService Dialog { get; set; }

    public Playground(IDrawing drawing, DialogService dialog)
    {
        (Drawing, Dialog) = (drawing, dialog);
    }
    public void CreateMenus(IJSRuntime js, NavigationManager nav)
    {
        Drawing.EstablishMenu<FoMenu2D>("Testing", new Dictionary<string, Action>()
        {

            { "Capture", () => CreateCapturePlayground()},
            { "Group", () => CreateGroupShape()},
            { "Ring", () => CreateGroupPlayground()},
            { "Glue", () => CreateGluePlayground()},
            { "Line", () => CreateLinePlayground()},
            //{ "Lets Dance", () => LetsDance()},
            { "Side Dialog", () => SideDialog()}
        }, true)
      .LayoutVertical(130, 40)
      .MoveTo(0, 370);
    }
    private void CreateCapturePlayground()
    {
        var s3 = Drawing.AddShape(new FoShape2D(100, 100, "Cyan"));
        s3.MoveTo(800, 200);
        var s4 = Drawing.AddShape(new FoShape2D(50, 50, "Green"));
        s4.MoveTo(10, 10);

        Drawing.CurrentPage().ExtractWhere<FoShape2D>(child => child == s4);
        s3.CaptureShape<FoShape2D>(s4, false);

        //SendShapeCreated<FoShape2D>(s3);
    }


    private void CreateGroupShape()
    {
        var c1 = Drawing.AddShape<FoGroup2D>(new FoGroup2D("", 100, 100, "Cyan")).MoveTo(1200, 90) as FoGroup2D;
        c1?.Add<FoShape2D>(new FoShape2D(20, 30, "Red"));
        c1?.Add<FoShape2D>(new FoShape2D(20, 30, "Green")).MoveTo(30, 20);
        c1?.CaptureShape<FoShape2D>(new FoShape2D(20, 30, "Blue")).MoveTo(100, 90);

        // if (c1 != null)
        //     SendShapeCreated<FoGroup2D>(c1);
    }


    private void CreateGluePlayground()
    {
        var s1 = Drawing.AddShape(new FoShape2D(200, 100, "Green"));
        s1.MoveTo(200, 200);


        var s2 = Drawing.AddShape(new FoShape2D(200, 100, "Orange"));
        s2.MoveTo(800, 400);

        var s3 = Drawing.AddShape(new FoShape2D(200, 100, "Blue"));
        s3.MoveTo(800, 200);

        var yyy = new FoShape1D(s1, s2, 10, "Red");
        var wire2 = Drawing.AddShape(yyy);

        var xxx = new FoShape1D(s1, s3, 10, "Yellow");
        var wire1 = Drawing.AddShape(xxx);

        // SendShapeCreated<FoShape2D>(s2);
        // SendShapeCreated<FoShape2D>(s1);
        // SendShapeCreated<FoShape2D>(s3);
        // SendShapeCreated<FoShape1D>(wire1);
        // SendShapeCreated<FoShape1D>(wire2);
    }

    private void CreateGroupPlayground()
    {
        var radius = 100;

        int cnt = 0;
        for (int i = 0; i <= 360; i += 30)
        {
            var a = Math.PI / 180.0 * i;
            var x = (int)(radius * Math.Cos(a)) + 1200;
            var y = (int)(radius * Math.Sin(a)) + 300;
            var shape = new FoShape2D(30, 30, "Cyan");
            shape.MoveTo(x, y);
            shape.ShapeDraw = (cnt++ % 3 == 0) ? shape.DrawCircle : shape.DrawRect;
            Drawing.AddShape<FoShape2D>(shape);
            //SendShapeCreated<FoShape2D>(shape);
        }
    }


    // private void LetsDance()
    // {
    //     var rand = new Random();
    //     var page = PageManager.CurrentPage();

    //     PageManager.Selections().ForEach(shape =>
    //     {
    //         //$"{shape.Name} Animations".WriteLine(ConsoleColor.DarkYellow);

    //         var X = page?.Width / 2 + 600 * (0.5 - rand.NextDouble());
    //         var Y = page?.Height / 2 + 600 * (0.5 - rand.NextDouble());
    //         Animations.Tween(shape, new { PinX = X, PinY = Y }, 2)
    //             .OnUpdate((arg) =>
    //             {
    //                 FoGlyph2D.ResetHitTesting = true;
    //                 //$"{arg} working".WriteLine(ConsoleColor.DarkYellow);
    //             });

    //         var factor = 3 * rand.NextDouble();
    //         Animations.Tween(shape, new { Width = factor * shape.Width, Height = factor * shape.Height }, 3);

    //         if (shape is FoImage2D)
    //             Animations.Tween(shape, new { ScaleX = 2 * rand.NextDouble(), ScaleY = 2 * rand.NextDouble() }, 3);

    //         var angle = shape.Angle + 360.0 * rand.NextDouble();
    //         Animations.Tween(shape, new { Angle = angle }, 4);
    //     });
    // }
    private void CreateLinePlayground()
    {
        Drawing.AddShape(new FoConnector1D(200, 200, 400, 400, "Red"));
        Drawing.AddShape(new FoConnector1D(200, 400, 400, 600, "Blue"));
    }
    public void SideDialog()
    {

        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoShape2D(150, 100, "Blue");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Drawing.AddShape<FoShape2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        //SendShapeCreated(target);
                        //SendToast(UserToast.Success("Created"));
                    }},
                    { "OnCancel", () => {
                        Drawing.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new SideDialogOptions() { CloseDialogOnOverlayClick = false, Position = DialogPosition.Right, ShowMask = false };
                await Dialog.OpenSideAsync<BlueRectangle>("Edit Hero", parmas, options);
            };

            shape.OpenCreate();

        });
    }

 
}
