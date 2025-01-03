open System
open System.IO
open System.Globalization

/// Опишем структуру для удобства
type ExchangeRecord = {
    Nominal : int
    Date    : DateTime
    Curs    : decimal
    Cdx     : string
}

[<EntryPoint>]
let main argv =

    // Путь к вашему csv-файлу
    let csvPath = "RC_F01_01_2014_T29_12_2024.csv"
    
    // Месячный платеж
    let monthlyPayment = 87_100m
    
    // Начальный взнос
    let entryFee = 2_000_000m

    // Считываем все строки (первая строка — заголовок, поэтому её пропускаем)
    let lines =
        File.ReadAllLines(csvPath)
        |> Array.skip 1  // Пропускаем заголовок: nominal,data,curs,cdx

    // Функция для разбора строки
    let parseLine (line : string) =
        let parts = line.Split(',')
        let nominal = Int32.Parse(parts.[0])
        let date    = DateTime.Parse(parts.[1], CultureInfo.InvariantCulture)
        let curs    = Decimal.Parse(parts.[2], CultureInfo.InvariantCulture)
        let cdx     = parts.[3]
        {
            Nominal = nominal
            Date    = date
            Curs    = curs
            Cdx     = cdx
        }

    // Парсим все строки в массив записей
    let records = lines |> Array.map parseLine

    // ----- Основная логика -----

    // 1. Определяем последнюю запись (по порядку в файле).
    //    Допустим, что "последняя" — это реально последняя строка в CSV.
    //    Если же нужно по самой свежей дате, то можно использовать maxBy (fun r -> r.Date).
    let lastRecord = records |> Array.last

    // 2. Пропускаем последнюю запись для расчёта конверсии 87 100 руб
    let recordsExceptLast = records |> Array.take (records.Length - 1)
    // или records |> Array.skipLast 1  (F# 6.0+)

    // 3. Конвертация 87 100 рублей по каждому "оставшемуся" курсу
    //    (т.е. без учёта последней записи)
    let converted =
        recordsExceptLast
        |> Array.map (fun r -> monthlyPayment / r.Curs)

    // 4. Сумма сконвертированных значений
    let totalSum = converted |> Array.sum

    // 5. Отдельно конвертируем начальный взнос  по "последней" записи
   
    let convertedEntry = entryFee / lastRecord.Curs

    // ----- Вывод результатов -----

   

  
    printfn "Общая сумма после конверсии (без последней записи): %M USD\n" totalSum

    
    printfn "Последняя запись: Дата = %O, Курс = %M" lastRecord.Date lastRecord.Curs
    printfn "Первый взнос -> %M USD (по последней записи)" convertedEntry
    
    //общая сумма включаяя последнюю запись
    printfn "Общая сумма включаяя последнюю запись: %M USD" (totalSum + convertedEntry)

    0 // Возвращаемое значение main
