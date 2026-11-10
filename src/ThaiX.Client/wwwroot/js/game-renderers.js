import { GAME_HEIGHT as H, GAME_WIDTH as W, GAME_COLORS as COLORS } from "./game-constants.js";

export function createGameRenderer(canvas, gameId, strings, getMinesLayout, isMinesMobileLayout) {
    const T = strings;
    const ctx = canvas.getContext("2d");
    let state;

    function drawText(text,x,y,size=22,color=COLORS.text,align="center") {
        ctx.fillStyle=color;ctx.font=`600 ${size}px system-ui, sans-serif`;ctx.textAlign=align;ctx.fillText(text,x,y);
    }
    function rounded(x,y,w,h,r,fill,stroke=null) {
        ctx.beginPath();ctx.roundRect(x,y,w,h,r);ctx.fillStyle=fill;ctx.fill();
        if(stroke){ctx.strokeStyle=stroke;ctx.stroke();}
    }
    function frame(title) {
        ctx.clearRect(0,0,W,H);ctx.fillStyle=COLORS.bg;ctx.fillRect(0,0,W,H);
        drawText(title,W/2,34,22);
    }
    function render(nextState) {
        state=nextState;
        if(!state)return;
        switch(gameId){
            case "2048": draw2048();break;case "minesweeper":drawMines();break;case "sudoku":drawSudoku();break;
            case "memory-cards":drawMemory();break;case "15-puzzle":drawFifteen();break;case "lights-out":drawLights();break;
            case "snake":drawSnake();break;case "tetris":drawTetris();break;case "flappy":drawFlappy();break;
            case "breakout":drawBreakout();break;case "pong":drawPong();break;case "wordle":drawWordle();break;
            case "hangman":drawHangman();break;case "typing-speed":drawTyping();break;case "tic-tac-toe":drawTicTacToe();break;
            case "connect-four":drawConnectFour();break;
        }
        const overlayCenterY=gameId==="minesweeper"?canvas.height/2:H/2;
        if(state.paused&&!state.finished){rounded(220,overlayCenterY-45,280,90,16,"#0b1220");drawText(T.paused,W/2,overlayCenterY+10,28);}
        if(state.finished){rounded(190,overlayCenterY-58,340,115,16,"#0b1220",COLORS.accent);drawText(T.gameOver,W/2,overlayCenterY-18,26);drawText(`${T.score}: ${state.score}`,W/2,overlayCenterY+22,22,COLORS.good);drawText(T.restartHint,W/2,overlayCenterY+82,15,COLORS.muted);}
    }
    function draw2048(){
        frame(T.title);const size=92,gap=10,left=134,top=70;
        state.grid.forEach((row,y)=>row.forEach((v,x)=>{const px=left+x*(size+gap),py=top+y*(size+gap);rounded(px,py,size,size,10,v?tileColor(v):COLORS.panel);if(v)drawText(String(v),px+size/2,py+57,26,v>=128?"#fff":COLORS.text);}));
        drawText(`${T.score}: ${state.score}`,W/2,475,18,COLORS.muted);
    }
    function tileColor(v){return ({2:"#64748b",4:"#0e7490",8:"#0284c7",16:"#2563eb",32:"#7c3aed",64:"#9333ea",128:"#c026d3",256:"#db2777",512:"#ea580c",1024:"#ca8a04",2048:"#16a34a"})[v]||"#15803d";}
    function drawMines(){
        const layout=getMinesLayout();
        const mobileLayout=mobileLayout;
        const {size,left,top}=layout;
        ctx.clearRect(0,0,W,canvas.height);
        ctx.fillStyle=COLORS.bg;
        ctx.fillRect(0,0,W,canvas.height);
        drawText(T.title,W/2,mobileLayout?44:34,mobileLayout?30:22);

        state.cells.forEach((row,y)=>row.forEach((cell,x)=>{
            const px=left+x*size,py=top+y*size;
            ctx.fillStyle=cell.open?"#cbd5e1":COLORS.tile;
            ctx.fillRect(px+1,py+1,size-2,size-2);
            ctx.strokeStyle="#64748b";
            ctx.strokeRect(px+1,py+1,size-2,size-2);

            if(cell.open){
                if(cell.mine)drawText("*",px+size/2,py+size*0.68,mobileLayout?40:23,COLORS.bad);
                else if(cell.count)drawText(String(cell.count),px+size/2,py+size*0.68,mobileLayout?30:18,["", "#2563eb","#16a34a","#dc2626","#7c3aed"][cell.count]||"#0f172a");
            }else if(cell.flag){
                drawText("F",px+size/2,py+size*0.68,mobileLayout?30:18,COLORS.warn);
            }

            if(state.cursor[0]===x&&state.cursor[1]===y){
                ctx.strokeStyle=COLORS.accent;
                ctx.lineWidth=mobileLayout?5:3;
                ctx.strokeRect(px+3,py+3,size-6,size-6);
                ctx.lineWidth=1;
            }
        }));

        drawText(`${T.flags}: ${state.flags}/10`,W/2,mobileLayout?770:465,mobileLayout?26:17,COLORS.muted);
    }
    function drawSudoku(){
        frame(T.title);const size=42,left=171,top=55;
        for(let y=0;y<9;y++)for(let x=0;x<9;x++){const px=left+x*size,py=top+y*size;ctx.fillStyle=state.selected[0]===x&&state.selected[1]===y?"#075985":(Math.floor(x/3)+Math.floor(y/3))%2?"#1e293b":"#273449";ctx.fillRect(px,py,size,size);ctx.strokeStyle="#64748b";ctx.strokeRect(px,py,size,size);if(state.grid[y][x])drawText(String(state.grid[y][x]),px+21,py+28,19,state.fixed[y][x]?"#f8fafc":"#7dd3fc");}
        ctx.strokeStyle="#e2e8f0";ctx.lineWidth=3;for(let i=0;i<=9;i+=3){ctx.strokeRect(left+i*size,top,1,9*size);ctx.strokeRect(left,top+i*size,9*size,1);}ctx.lineWidth=1;
        for(let i=1;i<=9;i++){const x=104+(i-1)*57;rounded(x,440,50,30,5,COLORS.tile);drawText(String(i),x+25,462,16);}
        rounded(615,440,50,30,5,COLORS.tile);drawText(T.deleteKey,640,462,12);
    }
    function drawMemory(){
        frame(T.title);const size=104,gap=12,left=150,top=70;
        state.cards.forEach((c,i)=>{const x=i%4,y=Math.floor(i/4),px=left+x*(size+gap),py=top+y*(size+gap);rounded(px,py,size,size,12,c.open||c.matched?"#e2e8f0":COLORS.tile);if(c.open||c.matched)drawText(String(c.value),px+size/2,py+62,30,"#0f172a");else drawText("?",px+size/2,py+62,30);if(state.cursor===i){ctx.strokeStyle=COLORS.accent;ctx.lineWidth=3;ctx.strokeRect(px+3,py+3,size-6,size-6);ctx.lineWidth=1;}});
        drawText(`${T.moves}: ${state.moves}`,W/2,475,18,COLORS.muted);
    }
    function drawFifteen(){
        frame(T.title);const size=96,gap=8,left=160,top=65;
        state.tiles.forEach((v,i)=>{const x=i%4,y=Math.floor(i/4),px=left+x*(size+gap),py=top+y*(size+gap);rounded(px,py,size,size,10,v?COLORS.tile:"#0b1220");if(v)drawText(String(v),px+48,py+58,27);if(state.cursor===i){ctx.strokeStyle=COLORS.accent;ctx.lineWidth=3;ctx.strokeRect(px+3,py+3,size-6,size-6);ctx.lineWidth=1;}});
        drawText(`${T.moves}: ${state.moves}`,W/2,475,18,COLORS.muted);
    }
    function drawLights(){
        frame(T.title);const size=66,gap=12,left=180,top=65;
        state.grid.forEach((row,y)=>row.forEach((v,x)=>{const px=left+x*(size+gap),py=top+y*(size+gap);rounded(px,py,size,size,12,v?"#facc15":"#334155");if(state.cursor[0]===x&&state.cursor[1]===y){ctx.strokeStyle=COLORS.accent;ctx.lineWidth=3;ctx.strokeRect(px+3,py+3,size-6,size-6);ctx.lineWidth=1;}}));
        drawText(`${T.moves}: ${state.moves}`,W/2,465,18,COLORS.muted);
    }
    function drawSnake(){
        frame(T.title);const size=25,left=110,top=58;
        ctx.fillStyle="#0b1220";ctx.fillRect(left,top,20*size,15*size);
        state.snake.forEach((p,i)=>{ctx.fillStyle=i?"#16a34a":"#86efac";ctx.fillRect(left+p[0]*size+1,top+p[1]*size+1,size-2,size-2);});
        ctx.fillStyle=COLORS.bad;ctx.fillRect(left+state.food[0]*size+4,top+state.food[1]*size+4,size-8,size-8);
        drawText(`${T.score}: ${state.score}`,W/2,465,18,COLORS.muted);
        [["^",650,345],["<",615,385],["v",650,425],[">",685,385]].forEach(([label,x,y])=>{rounded(x-15,y-15,30,30,6,COLORS.tile);drawText(label,x,y+6,18);});
    }
    function drawTetris(){
        frame(T.title);const size=20,left=230,top=42;
        ctx.fillStyle="#0b1220";ctx.fillRect(left,top,200,400);
        state.board.forEach((row,y)=>row.forEach((v,x)=>{if(v)rounded(left+x*size+1,top+y*size+1,size-2,size-2,3,"#38bdf8");}));
        state.piece.forEach((row,y)=>row.forEach((v,x)=>{if(v)rounded(left+(state.pieceX+x)*size+1,top+(state.pieceY+y)*size+1,size-2,size-2,3,"#f59e0b");}));
        drawText(`${T.score}: ${state.score}`,W/2,465,18,COLORS.muted);
        [["<",245],[T.rotateKey,315],[">",385],[T.dropKey,455]].forEach(([label,x])=>{rounded(x,438,62,30,5,COLORS.tile);drawText(label,x+31,459,13);});
    }
    function drawFlappy(){
        frame(T.title);ctx.fillStyle="#0c4a6e";ctx.fillRect(0,45,W,H-45);
        state.pipes.forEach(p=>{ctx.fillStyle="#22c55e";ctx.fillRect(p.x,45,55,p.gap-85-45);ctx.fillRect(p.x,p.gap+85,55,H-p.gap-85);});
        ctx.fillStyle="#facc15";ctx.beginPath();ctx.arc(105,state.birdY,15,0,Math.PI*2);ctx.fill();
        drawText(`${T.passed}: ${state.score}`,W/2,35,18);
    }
    function drawBreakout(){
        frame(T.title);state.bricks.forEach(b=>{if(b.alive)rounded(b.x,b.y,58,16,3,"#38bdf8");});
        rounded(state.paddleX,H-28,120,12,6,"#f8fafc");
        ctx.fillStyle="#facc15";ctx.beginPath();ctx.arc(state.ball.x,state.ball.y,8,0,Math.PI*2);ctx.fill();
        drawText(`${T.score}: ${state.score}`,W/2,465,18,COLORS.muted);
    }
    function drawPong(){
        frame(T.title);rounded(38,state.paddleY,12,100,6,"#38bdf8");rounded(W-50,state.aiY,12,100,6,"#f8fafc");
        ctx.fillStyle="#facc15";ctx.beginPath();ctx.arc(state.ball.x,state.ball.y,9,0,Math.PI*2);ctx.fill();
        drawText(`${state.player} : ${state.ai}`,W/2,70,28);
    }
    function getWordleStatuses(guess) {
        const statuses=Array(5).fill("absent"),remaining={};
        for(let i=0;i<5;i++){
            if(guess[i]===state.word[i])statuses[i]="correct";
            else remaining[state.word[i]]=(remaining[state.word[i]]||0)+1;
        }
        for(let i=0;i<5;i++){
            if(statuses[i]==="correct")continue;
            if(remaining[guess[i]]>0){statuses[i]="present";remaining[guess[i]]--;}
        }
        return statuses;
    }

    function drawWordle(){
        frame(T.title);const size=40,gap=6,left=220,top=45;
        for(let y=0;y<6;y++){
            const guess=state.guesses[y]||"";
            const statuses=guess?getWordleStatuses(guess):null;
            for(let x=0;x<5;x++){
                const px=left+x*(size+gap),py=top+y*(size+gap),ch=guess[x]|| (y===state.guesses.length?state.current[x]||"":"");
                let fill=COLORS.panel;
                if(statuses){const status=statuses[x];fill=status==="correct"?"#15803d":status==="present"?"#a16207":"#475569";}
                rounded(px,py,size,size,6,fill,COLORS.line);if(ch)drawText(ch,px+size/2,py+36,23);
            }
        }
        const rows=["QWERTYUIOP","ASDFGHJKL","ZXCVBNM"];
        rows.forEach((row,ri)=>[...row].forEach((letter,i)=>{const keyW=34,x=185+i*36+(ri===1?18:ri===2?54:0),y=345+ri*38;rounded(x,y,keyW,30,4,COLORS.tile);drawText(letter,x+keyW/2,y+21,14);}));
        rounded(545,421,65,28,4,COLORS.tile);drawText(T.deleteKey,577,440,12);
        rounded(615,421,65,28,4,COLORS.tile);drawText(T.enterKey,647,440,11);
    }
    function drawHangman(){
        frame(T.title);drawText([...state.word].map(ch=>state.guessed.includes(ch)?ch:"_").join(" "),W/2,190,40);
        drawText(`${T.wrongGuesses}: ${state.wrong}/6`,W/2,245,20,COLORS.warn);
        drawText(state.guessed.join("  "),W/2,310,20,COLORS.muted);
        const rows=["QWERTYUIOP","ASDFGHJKL","ZXCVBNM"];
        rows.forEach((row,ri)=>[...row].forEach((letter,i)=>{const keyW=34,x=185+i*36+(ri===1?18:ri===2?54:0),y=345+ri*38;rounded(x,y,keyW,30,4,COLORS.tile);drawText(letter,x+keyW/2,y+21,14);}));
    }
    function drawTyping(){
        frame(T.title);drawText(`${T.time}: ${state.seconds}s`,W/2,80,24,COLORS.warn);
        ctx.fillStyle=COLORS.text;ctx.font="20px system-ui";ctx.textAlign="left";
        const words=state.words;let y=145,x=75;ctx.fillStyle=COLORS.muted;
        words.forEach(word=>{const width=ctx.measureText(word+" ").width;if(x+width>W-75){x=75;y+=34;}ctx.fillStyle=state.typed.length>=state.text.indexOf(word)?COLORS.good:COLORS.muted;ctx.fillText(word,x,y);x+=width;});
        rounded(65,335,590,55,8,"#1e293b");ctx.fillStyle=COLORS.text;ctx.font="18px monospace";ctx.fillText(state.typed.slice(-65),80,368);
        drawText(T.typingStart,W/2,440,16,COLORS.muted);
    }
    function drawTicTacToe(){
        frame(T.title);const size=112,left=192,top=65;
        for(let y=0;y<3;y++)for(let x=0;x<3;x++){const index=y*3+x,px=left+x*size,py=top+y*size;rounded(px+4,py+4,size-8,size-8,12,COLORS.panel,COLORS.line);if(state.cells[index])drawText(state.cells[index],px+size/2,py+76,52,state.cells[index]==="X"?"#38bdf8":"#f59e0b");if(state.cursor===index){ctx.strokeStyle=COLORS.accent;ctx.lineWidth=3;ctx.strokeRect(px+7,py+7,size-14,size-14);ctx.lineWidth=1;}}
        drawText(T.youAreX,W/2,455,16,COLORS.muted);
    }
    function drawConnectFour(){
        frame(T.title);rounded(125,55,470,390,20,"#1d4ed8");
        drawText("V",158+state.cursor*67,52,22,COLORS.warn);
        for(let r=0;r<6;r++)for(let c=0;c<7;c++){ctx.fillStyle=state.board[r][c]===1?"#facc15":state.board[r][c]===2?"#f8fafc":COLORS.bg;ctx.beginPath();ctx.arc(158+c*67,90+r*61,24,0,Math.PI*2);ctx.fill();}
        drawText(T.youAreYellow,W/2,470,16,COLORS.muted);
    }


    return { render };
}
