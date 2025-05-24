document.addEventListener('DOMContentLoaded', function () {
    const data = window.statisticsData || {};

    function createChart(canvasId, chartType, labels, dataSet, label, yAxisTitle, colors = {}) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error("canvas element " + canvasId + " not found");
            return;
        }

        const ctx = canvas.getContext('2d');
        if (ctx) {
            new Chart(ctx, {
                type: chartType,
                data: {
                    labels: labels,
                    datasets: [{
                        label: label,
                        data: dataSet,
                        borderColor: colors.borderColor || 'rgba(75, 192, 192, 1)',
                        backgroundColor: colors.backgroundColor || 'rgba(75, 192, 192, 0.2)',
                        tension: 0.3,
                        fill: chartType === 'line',
                        borderWidth: 2,
                        pointRadius: 3
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true },
                        tooltip: { mode: 'index', intersect: false }
                    },
                    scales: {
                        x: {
                            title: {
                                display: true,
                                text: chartType === 'line' ? 'Thời gian' : 'Sự kiện'
                            }
                        },
                        y: {
                            title: { display: true, text: yAxisTitle },
                            beginAtZero: true
                        }
                    }
                }
            });
        } else {
            console.error(`Failed to get 2D context for '${canvasId}'.`);
        }
    }

    createChart(
        'registrationOverTimeChart',
        'line',
        data.registrationLabels,
        data.registrationData,
        'Số lượt đăng ký',
        'Số lượt đăng kí (Người)',
        { borderColor: 'rgba(75, 192, 192, 1)', backgroundColor: 'rgba(75, 192, 192, 0.2)' }
    );

    createChart(
        'DonationOverTimeChart',
        'line',
        data.donationLabels,
        data.donationData,
        'Số tiền ủng hộ',
        'Số tiền ủng hộ (VND)',
        { borderColor: 'rgba(75, 192, 192, 1)', backgroundColor: 'rgba(75, 192, 192, 0.2)' }
    );

    createChart(
        'registrationByEventChart',
        'bar',
        data.registrationByEventLabels,
        data.registrationByEventData,
        'Số lượt đăng ký',
        'Số lượt đăng ký (Người)',
        { borderColor: 'rgba(153, 102, 255, 1)', backgroundColor: 'rgba(153, 102, 255, 0.2)' }
    );

    createChart(
        'donationByEventChart',
        'bar',
        data.donationByEventLabels,
        data.donationByEventData,
        'Số tiền ủng hộ',
        'Số tiền ủng hộ (VND)',
        { borderColor: 'rgba(255, 206, 86, 1)', backgroundColor: 'rgba(255, 206, 86, 0.2)' }
    );
});