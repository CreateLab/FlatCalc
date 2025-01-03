open System
open System.IO
open System.Globalization

module CurrencyCalc =

    // --- Константы (магические числа) ---
    [<Literal>]
    let CsvPath = "RC_F01_01_2014_T29_12_2024.csv" // путь к файлу CSV

    [<Literal>]
    let SkipHeaderLines = 1 // сколько строк пропускаем для заголовка

    let FirstPayment = 2_000_000m // первый взнос
    let MonthlyPayment = 87_100m // ежемесячный платёж

    // Тип для удобства хранения строк CSV
    type ExchangeRecord =
        { Nominal: int
          Date: DateTime
          Curs: decimal
          Cdx: string }

open CurrencyCalc

[<EntryPoint>]
let main _ =
    // 1. Считываем данные
    let lines =
        File.ReadAllLines(CurrencyCalc.CsvPath)
        |> Array.skip CurrencyCalc.SkipHeaderLines // пропускаем заголовок CSV

    // Функция разбора строки
    let parseLine (line:string): ExchangeRecord =
        let parts = line.Split(',')
        let nominal = Int32.Parse(parts.[0])
        // Парсим дату
        let date = DateTime.Parse(parts.[1], CultureInfo.InvariantCulture)
        // Парсим курс (точка как разделитель)
        let curs = Decimal.Parse(parts.[2], CultureInfo.InvariantCulture)
        let cdx  = parts.[3]
        {
            Nominal = nominal
            Date    = date
            Curs    = curs
            Cdx     = cdx
        }

    // 2. Парсим строки и (при необходимости) пропускаем первую запись данных
    let allRecords = lines |> Array.map parseLine
    let records = allRecords |> Array.skip 1 // Пропустить первую запись, если так сказано в задаче

    // 3. Группировка по (год, месяц), сортировка
    let groupedByMonth =
        records
        |> Array.groupBy (fun r -> r.Date.Year, r.Date.Month)
        |> Array.sortBy (fun ((year, month), _) -> year, month)

    // 4. Для самого раннего месяца берём первый взнос (2 000 000 руб),
    //    для остальных — ежемесячный платёж (87 100 руб).
    //    Считаем средний курс, конвертируем.
    let monthlyUsd =
        groupedByMonth
        |> Array.mapi (fun i ((year, month), recsInMonth) ->
            let avgRate = recsInMonth |> Array.averageBy (fun r -> float r.Curs) |> decimal

            let rubAmount =
                if i = 0 then
                    CurrencyCalc.FirstPayment
                else
                    CurrencyCalc.MonthlyPayment

            rubAmount / avgRate)

    // 5. Суммируем все результаты и выводим в консоль.
    let totalUsd = monthlyUsd |> Array.sum
    printfn "Итоговая сумма в долларах: %M" totalUsd

    0
