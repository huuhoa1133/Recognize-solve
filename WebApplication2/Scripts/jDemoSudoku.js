﻿var w12 = new Array(156800);
var w23 = [];
var b2 = [];
var b3 = [];

var checkloaddata = 0;
function checkLoadWeight() {
    if (checkloaddata == 4) {
        alert("Load data success");
    } else {
        checkloaddata++;
    }
}
//w12
$.ajax({
    url: '/Sudoku/GetValue',
    dataType: "json",
    type: "POST",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({ type: 1, typew12: 1 }),
    async: true,
    processData: false,
    cache: false,
    success: function (data) {
        for (var i = 0; i < 78400; i++) {
            w12[i] = data[i];
        }
        checkLoadWeight();
    },
    error: function (xhr) {
        alert('error: ' + xhr.responseText);
    }
});
$.ajax({
    url: '/Sudoku/GetValue',
    dataType: "json",
    type: "POST",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({ type: 1, typew12: 2 }),
    async: true,
    processData: false,
    cache: false,
    success: function (data) {
        for (var i = 0; i < 78400; i++) {
            w12[i + 78400] = data[i];
        }

        checkLoadWeight();
    },
    error: function (xhr) {
        alert('error: ' + xhr.responseText);
    }
});
//bias2
$.ajax({
    url: '/Sudoku/GetValue',
    dataType: "json",
    type: "POST",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({ type: 2, typew12: 1 }),
    async: true,
    processData: false,
    cache: false,
    success: function (data) {
        for (var i = 0; i < 200; i++) {
            b2[i] = data[i];
        }
        checkLoadWeight();
    },
    error: function (xhr) {
        alert('error: ' + xhr.responseText);
    }
});
//w23
$.ajax({
    url: '/Sudoku/GetValue',
    dataType: "json",
    type: "POST",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({ type: 3, typew12: 1 }),
    async: true,
    processData: false,
    cache: false,
    success: function (data) {

        for (var i = 0; i < 2000; i++) {
            w23[i] = data[i];
        }
        checkLoadWeight();
    },
    error: function (xhr) {
        alert('error: ' + xhr.responseText);
    }
});
//bias3
$.ajax({
    url: '/Sudoku/GetValue',
    dataType: "json",
    type: "POST",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({ type: 4, typew12: 1 }),
    async: true,
    processData: false,
    cache: false,
    success: function (data) {
        for (var i = 0; i < 10; i++) {
            b3[i] = data[i];
        }
        checkLoadWeight();
    },
    error: function (xhr) {
        alert('error: ' + xhr.responseText);
    }
});

var canvas;
var ctx;

var prevX = 0;
var currX = 0;
var prevY = 0;
var currY = 0;
var paths = []; // recording paths
var paintFlag = false;
var color = "black";
var lineWidth = 20;

var clearBeforeDraw = false; // controls whether canvas will be cleared on next mousedown event. Set to true after digit recognition

// the neural network's weights (unit-unit weights, and unit biases)
// training was done in Matlab with the MNIST dataset.
// this data is for a 784-200-10 unit, with logistic non-linearity in the hidden and softmax in the output layer.
// The input is a [-1;1] gray level image, background == 1, 28x28 pixels linearized in column order (i.e. column1(:); column2(:); ...)
// i-th output being the maximum means the network thinks the input encodes (i-1)
// the weights below showed a 1.92% error rate on the test data set (9808/10000 digits recognized correctly).

//neural net with one hidden layer; sigmoid for hidden, softmax for output
function nn(data, w12, bias2, w23, bias3) {

    var t1 = new Date();

    // compute layer2 output
    var out2 = [];
    for (var i = 0; i < 200; i++) {
        out2[i] = bias2[i];
        for (var j = 0; j < 784; j++) {
            out2[i] += data[j] * w12[i * 784 + j];
        }
        //out2[i] = 1 / (1 + Math.exp(-out2[i]));
        out2[i] = Tanh(out2[i]);
    }
    //compute layer3 activation
    var out3 = [];
    for (var i = 0; i < 10; i++) {
        out3[i] = bias3[i];
        for (var j = 0; j < 200; j++) {
            out3[i] += out2[j] * w23[i * 200 + j];
        }
        out3[i] = 1 / (1 + Math.exp(-out3[i]));
    }
    // compute layer3 output (softmax)
    //var max3 = out3.reduce(function (p, c) { return p > c ? p : c; });

    //var nominators = out3.map(function (e) { return Math.exp(e - max3); });
    //var denominator = nominators.reduce(function (p, c) { return p + c; });
    //var output = nominators.map(function (e) { return e / denominator; });

    // timing measurement
    var dt = new Date() - t1; console.log('NN time: ' + dt + 'ms');
    return out3;
}
function Tanh(x) {
    if (x > 45) return 1;
    else if (x < -45) return -1;
    else return ((Math.exp(x) - Math.exp(-x)) / (Math.exp(x) + Math.exp(-x)));
}
// computes center of mass of digit, for centering
// note 1 stands for black (0 white) so we have to invert.
function centerImage(img) {
    var meanX = 0;
    var meanY = 0;
    var rows = img.length;
    var columns = img[0].length;
    var sumPixels = 0;
    for (var y = 0; y < rows; y++) {
        for (var x = 0; x < columns; x++) {
            var pixel = (1 - img[y][x]);
            sumPixels += pixel;
            meanY += y * pixel;
            meanX += x * pixel;
        }
    }
    meanX /= sumPixels;
    meanY /= sumPixels;

    var dY = Math.round(rows / 2 - meanY);
    var dX = Math.round(columns / 2 - meanX);
    return { transX: dX, transY: dY };
}

// given grayscale image, find bounding rectangle of digit defined
// by above-threshold surrounding
function getBoundingRectangle(img, threshold) {
    var rows = img.length;
    var columns = img[0].length;
    var minX = columns;
    var minY = rows;
    var maxX = -1;
    var maxY = -1;
    for (var y = 0; y < rows; y++) {
        for (var x = 0; x < columns; x++) {
            if (img[y][x] < threshold) {
                if (minX > x) minX = x;
                if (maxX < x) maxX = x;
                if (minY > y) minY = y;
                if (maxY < y) maxY = y;
            }
        }
    }
    return { minY: minY, minX: minX, maxY: maxY, maxX: maxX };
}

// take canvas image and convert to grayscale. Mainly because my
// own functions operate easier on grayscale, but some stuff like
// resizing and translating is better done with the canvas functions
function imageDataToGrayscale(imgData) {
    var grayscaleImg = [];
    for (var y = 0; y < imgData.height; y++) {
        grayscaleImg[y] = [];
        for (var x = 0; x < imgData.width; x++) {
            var offset = y * 4 * imgData.width + 4 * x;
            var alpha = imgData.data[offset + 3];
            // weird: when painting with stroke, alpha == 0 means white;
            // alpha > 0 is a grayscale value; in that case I simply take the R value
            if (alpha == 0) {
                imgData.data[offset] = 255;
                imgData.data[offset + 1] = 255;
                imgData.data[offset + 2] = 255;
            }
            imgData.data[offset + 3] = 255;
            // simply take red channel value. Not correct, but works for
            // black or white images.
            grayscaleImg[y][x] = imgData.data[y * 4 * imgData.width + x * 4 + 0] / 255;
        }
    }
    return grayscaleImg;
}


//function convert matrix
function convertmatrix(nninput) {
    //left - right
    for (var i = 0; i < 28; i++) {
        for (var j = 0; j < 14 ; j++) {
            var temp = nninput[i * 28 + j];
            nninput[i * 28 + j] = nninput[i * 28 + 27 - j];
            nninput[i * 28 + 27 - j] = temp;
        }
    }
    //xoay
    var arr = [];
    for (var i = 0; i < 28; i++) {
        arr[i] = [];
        for (var j = 0; j < 28; j++) {

            arr[i][j] = nninput[i * 28 + j];
        }
    }


    var k = 0;
    for (var i = 27; i >= 0; i--) {
        for (var j = 0; j < 28 ; j++) {
            nninput[k] = arr[j][i];
            k++;
        }

    }


    return nninput;
}
// takes the image in the canvas, centers & resizes it, then puts into 10x10 px bins
// to give a 28x28 grayscale image; then, computes class probability via neural network
function recognize() {
    var t1 = new Date();

    // convert RGBA image to a grayscale array, then compute bounding rectangle and center of mass
    var imgData = ctx.getImageData(0, 0, 280, 280);
    grayscaleImg = imageDataToGrayscale(imgData);
    var boundingRectangle = getBoundingRectangle(grayscaleImg, 0.01);
    var trans = centerImage(grayscaleImg); // [dX, dY] to center of mass

    // copy image to hidden canvas, translate to center-of-mass, then
    // scale to fit into a 200x200 box (see MNIST calibration notes on
    // Yann LeCun's website)
    var canvasCopy = document.createElement("canvas");
    canvasCopy.width = imgData.width;
    canvasCopy.height = imgData.height;
    var copyCtx = canvasCopy.getContext("2d");
    var brW = boundingRectangle.maxX + 1 - boundingRectangle.minX;
    var brH = boundingRectangle.maxY + 1 - boundingRectangle.minY;
    var scaling = 190 / (brW > brH ? brW : brH);
    // scale
    copyCtx.translate(canvas.width / 2, canvas.height / 2);
    copyCtx.scale(scaling, scaling);
    copyCtx.translate(-canvas.width / 2, -canvas.height / 2);
    // translate to center of mass
    copyCtx.translate(trans.transX, trans.transY);


    copyCtx.drawImage(ctx.canvas, 0, 0);


    // now bin image into 10x10 blocks (giving a 28x28 image)
    imgData = copyCtx.getImageData(0, 0, 280, 280);
    grayscaleImg = imageDataToGrayscale(imgData);
    var nnInput = new Array(784);
    for (var y = 0; y < 28; y++) {
        for (var x = 0; x < 28; x++) {
            var mean = 0;
            for (var v = 0; v < 10; v++) {
                for (var h = 0; h < 10; h++) {
                    mean += grayscaleImg[y * 10 + v][x * 10 + h];
                }
            }
            mean = (1 - mean / 100); // average and invert
            nnInput[x * 28 + y] = mean;
            //if (mean > 50) nnInput[x * 28 + y] = 0;
            //else nnInput[x * 28 + y] = 1;
        }
    }

    //for copy & pasting the digit into matlab
    //document.getElementById('nnInput').innerHTML=JSON.stringify(nnInput)+';<br><br><br><br>';
    var maxIndex = 0;

    nnInput = convertmatrix(nnInput);

    //var str = '';
    //for (var i = 0; i < 28; i++) {
    //    for (var j = 0; j < 28; j++) {
    //        str += nnInput[i * 28 + j] + " ";
    //    }
    //    str += "\n";
    //}
    //alert(str);

    var nnOutput = nn(nnInput, w12, b2, w23, b3);
    console.log(nnOutput);
    nnOutput.reduce(function (p, c, i) { if (p < c) { maxIndex = i; return c; } else return p; });
    console.log('maxIndex: ' + maxIndex);
    //document.getElementById('nnOut').innerHTML = maxIndex;
    setDigit(maxIndex);
    clearBeforeDraw = true;
    var dt = new Date() - t1;
    console.log('recognize time: ' + dt + 'ms');
}

function init() {
    canvas = document.getElementById('can');
    ctx = canvas.getContext("2d");

    canvas.addEventListener("mousemove", function (e) {
        findxy('move', e)
    }, false);
    canvas.addEventListener("mousedown", function (e) {
        findxy('down', e)
    }, false);
    canvas.addEventListener("mouseup", function (e) {
        findxy('up', e)
    }, false);
    canvas.addEventListener("mouseout", function (e) {
        findxy('out', e)
    }, false);
}

// draws a line from (x1, y1) to (x2, y2) with nice rounded caps
function draw(ctx, color, lineWidth, x1, y1, x2, y2) {
    ctx.beginPath();
    ctx.strokeStyle = color;
    ctx.lineWidth = lineWidth;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
    ctx.closePath();
}

function erase() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    //document.getElementById('nnOut').innerHTML = '';
}

function findxy(res, e) {
    if (res == 'down') {
        if (clearBeforeDraw == true) {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            //document.getElementById('nnOut').innerHTML = '';
            paths = [];
            clearBeforeDraw = false;
        }

        if (e.pageX != undefined && e.pageY != undefined) {
            //currX = e.pageX - canvas.offsetLeft;
            //currY = e.pageY - canvas.offsetTop;
            var rect = can.getBoundingClientRect();
            currX = e.clientX - rect.left;
            currY = e.clientY - rect.top;
        } else {
            currX = e.clientX + document.body.scrollLeft
            + document.documentElement.scrollLeft
            - canvas.offsetLeft;
            currY = e.clientY + document.body.scrollTop
            + document.documentElement.scrollTop
            - canvas.offsetTop;
        }
        //draw a circle
        ctx.beginPath();
        ctx.lineWidth = 1;
        ctx.arc(currX, currY, lineWidth / 2, 0, 2 * Math.PI);
        ctx.stroke();
        ctx.closePath();
        ctx.fill();

        paths.push([[currX], [currY]]);
        paintFlag = true;
    }
    if (res == 'up' || res == "out") {
        paintFlag = false;
        //console.log(paths);
    }

    if (res == 'move') {
        if (paintFlag) {
            // draw a line to previous point
            prevX = currX;
            prevY = currY;
            if (e.pageX != undefined && e.pageY != undefined) {
                //currX = e.pageX - canvas.offsetLeft;
                //currY = e.pageY - canvas.offsetTop;
                var rect = can.getBoundingClientRect();
                currX = e.clientX - rect.left;
                currY = e.clientY - rect.top;
            } else {
                currX = e.clientX + document.body.scrollLeft
                + document.documentElement.scrollLeft
                - canvas.offsetLeft;
                currY = e.clientY + document.body.scrollTop
                + document.documentElement.scrollTop
                - canvas.offsetTop;
            }
            currPath = paths[paths.length - 1];
            currPath[0].push(currX);
            currPath[1].push(currY);
            paths[paths.length - 1] = currPath;
            draw(ctx, color, lineWidth, prevX, prevY, currX, currY);
        }
    }
}
init();

/////////////////////////////////////////////////////  SUDOKU  /////////////////
var matrixSudoku;
var digCellSize = 54;
var CellSize = 60;
var SquareSize = 3;
var BoardSize = SquareSize * SquareSize;
var canvas1 = document.getElementById("canvas1");
var selected = { x: -1, y: -1 };
var numHidden = 10;//so luong vi tri an

canvas1.onmousedown = function canvasMouseDown(ev) {
    var rect = canvas1.getBoundingClientRect();
    currX = ev.clientX - rect.left;
    currY = ev.clientY - rect.top;
    selected = { x: Math.floor(currX / CellSize), y: Math.floor(currY / CellSize) };
    updateUI();
}
function is_win() {
    for (var i = 0; i < BoardSize; i++) {
        for (var j = 0; j < BoardSize; j++) {
            if (matrixSudoku[i][j] == 0) {
                
                document.getElementById("alert-win").setAttribute('class', 'hidden');
                return;
            }
        }
    }
    document.getElementById("alert-win").setAttribute('class', 'visible alert alert-success');

}
function setDigit(value) {
    if (is_safe(selected.y, selected.x, value)) {
        matrixSudoku[selected.y][selected.x] = value;
        updateUI();
        is_win();
    }
}
function drawGrid() {
    // Only ever called once!
    var context = canvas1.getContext('2d');
    context.strokeStyle = '#808080';
    for (var i = 0; i <= BoardSize; i++) {
        context.beginPath();
        var thick = i % 3 == 0;
        // Draw vertical lines
        context.lineWidth = thick ? 2 : 1;
        context.moveTo(i * CellSize + 0.5, 0.5);
        context.lineTo(i * CellSize + 0.5, BoardSize * CellSize + 0.5);

        // Draw horizontal lines
        context.moveTo(0.5, i * CellSize + 0.5);
        context.lineTo(BoardSize * CellSize + 0.5, i * CellSize + 0.5);
        context.stroke();
    }
}
function drawCells() {
    var context = canvas1.getContext('2d');
    context.font = "32pt Calibri";
    context.textAlign = "center";
    context.textBaseline = "middle";
    var normalForeColor = "#191929";
    context.fillStyle = normalForeColor;
    for (var row = 0; row < BoardSize; row++) {
        for (var col = 0; col < BoardSize; col++) {
            //fill background selected
            if (row == selected.y && col == selected.x) {
                var margin = 2;
                context.beginPath();
                context.rect(col * CellSize + margin + 0.5, row * CellSize + margin + 0.5, CellSize - 2 * margin, CellSize - 2 * margin);
                context.fillStyle = "#ffe4e1";
                context.fill()
                context.fillStyle = normalForeColor;
            }
            //draw digit
            var x = (col + 0.5) * CellSize; // center of cell for textAlign center, textBaseline middle
            var y = (row + 0.5) * CellSize;


            var value = matrixSudoku[row][col];
            if(value != 0)
                context.fillText(value, x, y);

            
        }
    }
}
function updateUI() {
    canvas1.width = canvas1.width;
    drawGrid();
    drawCells();
}


function find_cell() {
    for (var i = 0; i < BoardSize; i++) {
        for (var j = 0; j < BoardSize; j++) {
            if (matrixSudoku[i][j] == 0) {
                return { i: i, j: j };
            }
        }
    }
    return { i: -1, j: -1 };
}
function col_safe(row,col,n){
    for (var i = 0; i < BoardSize; i++) {
        if(matrixSudoku[i][col] == n && i != row){
            return true;
        }
    }
    return false;
}
function row_safe(row,col,n){
    for (var i = 0; i < BoardSize; i++) {
        if(matrixSudoku[row][i] == n && i != col){
            return true;
        }
    }
    return false;
}
function sector_safe(row,col,n){
    for (var i = 0; i < SquareSize; i++) {
        for(var j = 0;j< SquareSize;j++){
            if(matrixSudoku[i+row][j+col] == n){
                return true;
            }
        }
    }
    return false;
}
function is_safe(row,col,n){
    if(!col_safe(row,col,n) && !row_safe(row,col,n)&&!sector_safe(row-row%3,col-col%3,n)){
        return true;
    }
    return false;
}
function FindSold() {
    
    var cell = find_cell();
    if (cell.i == -1) return true;
    for (var i = 1; i <= BoardSize; i++) {
        if (is_safe(cell.i, cell.j, i)) {
            matrixSudoku[cell.i][cell.j] = i;
            if (FindSold()) {
                return true;
            }
            matrixSudoku[cell.i][cell.j] = 0;
        }
    }

}
function solve() {
    FindSold();
    updateUI();
}
function reset() {
    GenSudoku();   
}
function GenSudoku() {
    document.getElementById("alert-win").setAttribute('class', 'hidden');
    matrixSudoku = [];
    for (var i = 0; i < BoardSize; i++) {
        matrixSudoku[i] = [];
    }
    for (var i = 0; i < BoardSize; i++) {
        for (var j = 0; j < BoardSize; j++) {
            matrixSudoku[i][j] = 0;
        }
    }
    matrixSudoku[0][0] = Math.floor((Math.random() * 9) + 1);//random 1-9
    FindSold();
    //hidden
    for (var i = 0; i < numHidden; i++) {
        var tempLoc = Math.floor(Math.random() * 80);
        var y = Math.floor( tempLoc/ BoardSize);
        var x = tempLoc % BoardSize;
        if (matrixSudoku[y][x] != 0) {
            matrixSudoku[y][x] = 0;
        } else {
            i--;
        }
    }
    updateUI();
}
GenSudoku();
function earse() {
    var can = document.getElementById('can');
    var context = can.getContext('2d');
    context.clearRect(0, 0, can.width, can.height);
}
function setlevel(value) {
    numHidden = value;
}





