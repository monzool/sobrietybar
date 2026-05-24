namespace Examples.CounterApp

// FuncUI DSL extension for LiveCharts CartesianChart
module LiveChartsDsl =
    open System.Collections.Generic
    open Avalonia.FuncUI.Types
    open Avalonia.FuncUI.DSL
    open Avalonia.FuncUI.Builder
    open LiveChartsCore
    open LiveChartsCore.Kernel.Sketches
    open LiveChartsCore.Measure
    open LiveChartsCore.SkiaSharpView.Avalonia

    type CartesianChart with
        static member create(attrs: IAttr<CartesianChart> list) : IView<CartesianChart> =
            ViewBuilder.Create<CartesianChart>(attrs)

        static member series<'t when 't :> CartesianChart>(value: IEnumerable<ISeries>) : IAttr<'t> =
            AttrBuilder<'t>.CreateProperty<IEnumerable<ISeries>>(
                CartesianChart.SeriesProperty, value, ValueNone)

        static member yAxes<'t when 't :> CartesianChart>(value: IEnumerable<ICartesianAxis>) : IAttr<'t> =
            AttrBuilder<'t>.CreateProperty<IEnumerable<ICartesianAxis>>(
                CartesianChart.YAxesProperty, value, ValueNone)

        static member tooltipPosition<'t when 't :> CartesianChart>(value: TooltipPosition) : IAttr<'t> =
            AttrBuilder<'t>.CreateProperty<TooltipPosition>(
                CartesianChart.TooltipPositionProperty, value, ValueNone)


module Main =
    open System
    open Avalonia.Controls
    open Avalonia.FuncUI
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open LiveChartsCore
    open LiveChartsCore.Measure
    open LiveChartsCore.SkiaSharpView
    open LiveChartsCore.SkiaSharpView.Avalonia
    open LiveChartsCore.SkiaSharpView.Painting
    open LiveChartsCore.Themes
    open LiveChartsDsl

    type Pilot = {
        Name: string
        Score: float
        Paint: SolidColorPaint
    }

    let private rng = Random()

    let private initPilots () =
        let paints =
            [| 0..6 |]
            |> Array.map (fun i ->
                SolidColorPaint(ColorPalletes.MaterialDesign500.[i].AsSKColor()))
        [|
            { Name = "Tsunoda";    Score = 500.0;  Paint = paints.[0] }
            { Name = "Sainz";      Score = 450.0;  Paint = paints.[1] }
            { Name = "Ricciardo";  Score = 520.0;  Paint = paints.[2] }
            { Name = "Bottas";     Score = 550.0;  Paint = paints.[3] }
            { Name = "Perez";      Score = 660.0;  Paint = paints.[4] }
            { Name = "Verstappen"; Score = 920.0;  Paint = paints.[5] }
            { Name = "Hamilton";   Score = 1000.0; Paint = paints.[6] }
        |]

    // One RowSeries per pilot so each bar gets its own colour via Fill.
    // The sort order of the array determines vertical bar positions (bottom to top).
    let private makeSeries (pilots: Pilot[]) : ISeries[] =
        pilots |> Array.map (fun p ->
            let s = RowSeries<float>()
            s.Values     <- [| p.Score |]
            s.Name       <- p.Name
            s.Fill       <- p.Paint
            s.DataLabelsFormatter <- Func<_,string>(fun _ -> p.Name)
            s.ShowDataLabels     <- true
            s.DataLabelsPosition <- DataLabelsPosition.End
            s.MaxBarWidth <- 50.0
            s.Padding     <- 5.0
            s :> ISeries)

    let view =
        Component (fun ctx ->
            let pilots = ctx.useState (initPilots ())

            let onRaceStep () =
                pilots.Current
                |> Array.map (fun p -> { p with Score = p.Score + float (rng.Next(0, 100)) })
                |> Array.sortBy (fun p -> p.Score)
                |> pilots.Set

            let hiddenYAxis =
                let a = Axis()
                a.IsVisible <- false
                a

            DockPanel.create [
                DockPanel.children [
                    Button.create [
                        Button.dock Dock.Bottom
                        Button.onClick (fun _ -> onRaceStep ())
                        Button.content "Race Step"
                        Button.horizontalAlignment HorizontalAlignment.Stretch
                    ]
                    CartesianChart.create [
                        CartesianChart.series (makeSeries pilots.Current)
                        CartesianChart.tooltipPosition TooltipPosition.Hidden
                        CartesianChart.yAxes [| hiddenYAxis |]
                    ]
                ]
            ]
        )
