function getChart(dates, prices, vols, avgprice, avgvol) {
    var ctx = document.getElementById("myChart").getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dates.split(","),
            datasets: [{
                label: 'High Prices',
                yAxisID: 'H',
                data: prices.split(","),
                type: 'line',
                borderColor: 'rgba(0, 103, 71, 1)',
                backgroundColor: 'rgba(0, 103, 71, 0.1)',
                lineTension: 0
            },
            {
                label: 'Volumes (Mn)',
                data: vols.split(","),
                borderColor: 'rgba(0, 0, 250, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: false,
            scales: {
                yAxes: [{
                    id: 'H',
                    type: 'linear',
                    position: 'left',
                }]
            },
            annotation: {
                drawTime: 'afterDatasetsDraw',
                annotations: [
                    {
                        id: 'highprice',
                        type: 'line',
                        mode: 'horizontal',
                        scaleID: 'H',
                        value: avgprice,
                        borderColor: 'green',
                        borderWidth: 1,
                        label: {
                            backgroundColor: "green",
                            content: "Mean: $" + avgprice,
                            enabled: true
                        }
                    },
                    {
                        id: 'volume',
                        type: 'line',
                        mode: 'horizontal',
                        scaleID: 'H',
                        value: avgvol,
                        borderColor: 'blue',
                        borderWidth: 1,
                        label: {
                            backgroundColor: "blue",
                            content: "Mean Volume: " + avgvol + "(Mn)",
                            enabled: true
                        }
                    }]
            }
        }
    });
}


function getStockChart(Open, High, Low, Close, Dates) {
    //var data = getRandomData('April 01 2017', 100);
    
    var dates = Dates.split(",");
    var open = Open.split(",");
    var high = High.split(",");
    var low = Low.split(",");
    var close = Close.split(",");
    
    var data = new Array();
    for(var i = 0; i < dates.length; i++)
    {
        if(parseInt(open[i]) != 0){
            bar = {o: parseFloat(open[i]), h: parseFloat(high[i]), l: parseFloat(low[i]), c: parseFloat(close[i]), t: dates[i]};
            data.push(bar);
        }
    }

    var ctx2 = document.getElementById("StockChart").getContext("2d");
        ctx2.canvas.width = 1000;
        ctx2.canvas.height = 250;
        new Chart(ctx2, {
            type: 'candlestick',
            data: {
                datasets: [{
                    label: "Corporation K line Graph",
                    data: data,
                    fractionalDigitsCount: 2,
                    backgroundColor: 'rgba(0, 103, 71, 0.1)'
                }]
            },
            options: {
                tooltips: {
                    position: 'nearest',
                    mode: 'index',
                },
            },
        });
}




function alertDbSave(success) {
    if (success === 1) {
        alert("Data saved successfully");
    }
}

function alertRepSave(success) {
    if (success === 1) {
        alert("Stock is added to your repositpry successfully")
    }
}