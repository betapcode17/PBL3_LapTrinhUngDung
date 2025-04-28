const ctx = document.getElementById("barChart").getContext("2d");
new Chart(ctx, {
  type: "bar",
  data: {
    labels: ["2020", "2021", "2022", "2023", "2024", "2025"],
    datasets: [
      //Number of volunteers
      {
        label: "",
        data: [4, 6, 2, 8, 10, 21],
        // backgroundColor: [
        //   "rgba(255, 99, 132, 0.3)",
        //   "rgba(54, 162, 235, 0.3)",
        //   "rgba(255, 206, 86, 0.3)",
        //   "rgba(75, 192, 192, 0.3)",
        //   "rgba(153, 102, 255, 0.3)",
        //   "rgba(255, 159, 64, 0.3)",
        // ],
        
        // borderColor: [
        //   "rgba(255, 99, 132, 1)",
        //   "rgba(54, 162, 235, 1)",
        //   "rgba(255, 206, 86, 1)",
        //   "rgba(75, 192, 192, 1)",
        //   "rgba(153, 102, 255, 1)",
        //   "rgba(255, 159, 64, 1)",
        // ],
        backgroundColor: "rgba(255, 99, 132, 0.3)",
        borderColor: "rgba(255, 99, 132, 1)",
        
        borderWidth: 1,
      },
    ],
  },
  options: {
    responsive: true,
    legend: {
      display: false,
    },
    scales: {
      xAxes: [{
        scaleLabel: {
          display: true,
          labelString: 'Year',
          fontColor: '#6c7293',
          fontSize: 12,
          fontStyle: 'bold'
        },
        gridLines: {
          display: true,
          color: 'rgba(204, 204, 204, 0.1)'
        }
      }],

      yAxes: [{
        scaleLabel: {
          display: true,
          labelString: 'Events',
          fontColor: '#6c7293',
          fontSize: 12,
          fontStyle: 'bold'
        },
        gridLines: {
          display: true,
          color: 'rgba(204, 204, 204, 0.1)'
        },
      
        ticks: {
          beginAtZero: true,
          min: 0,
        }
      }]
    },
  },
});