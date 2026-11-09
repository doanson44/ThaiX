const W = 720;
const H = 480;
const COLORS = {
    bg: "#101827", panel: "#1e293b", line: "#475569", text: "#f8fafc",
    muted: "#94a3b8", accent: "#38bdf8", good: "#22c55e", warn: "#f59e0b",
    bad: "#ef4444", tile: "#334155"
};
const SUDOKU_PUZZLE = [
    "530070000","600195000","098000060","800060003","400803001",
    "700020006","060000280","000419005","000080079"
];
const SUDOKU_SOLUTION = [
    "534678912","672195348","198342567","859761423","426853791",
    "713924856","961537284","287419635","345286179"
];
const TETRIS_SHAPES = [
    [[1,1,1,1]], [[1,1],[1,1]], [[0,1,0],[1,1,1]], [[1,0,0],[1,1,1]],
    [[0,0,1],[1,1,1]], [[1,1,0],[0,1,1]], [[0,1,1],[1,1,0]]
];

export function mount(canvas, gameId, dotnet, typingField = null, strings = {}) {
    const T = strings;
    const ctx = canvas.getContext("2d");
    let state;
    let timer = 0;
    let mismatchTimeout = 0;
    let animationFrame = 0;
    let lastFrame = 0;
    let pointerStart = null;
    let disposed = false;

    canvas.width = W;
    canvas.height = H;
    canvas.style.touchAction = "none";
    canvas.focus();

    function baseState() {
        return { startedAt: Date.now(), finished: false, paused: false, score: 0, moves: 0, mode: null };
    }

    function finish(score, mode = state.mode) {
        if (state.finished) return;
        state.finished = true;
        state.score = score;
        const duration = Math.max(1, Math.round((Date.now() - state.startedAt) / 1000));
        dotnet.invokeMethodAsync("OnGameCompleted", score, duration, mode);
        render();
    }

    function reset() {
        clearInterval(timer);
        clearTimeout(mismatchTimeout);
        mismatchTimeout = 0;
        cancelAnimationFrame(animationFrame);
        state = baseState();
        if (typeof typingField?.addEventListener === "function") typingField.value = "";
        switch (gameId) {
            case "2048": init2048(); break;
            case "minesweeper": initMines(); break;
            case "sudoku": initSudoku(); break;
            case "memory-cards": initMemory(); break;
            case "15-puzzle": initFifteen(); break;
            case "lights-out": initLights(); break;
            case "snake": initSnake(); break;
            case "tetris": initTetris(); break;
            case "flappy": initFlappy(); break;
            case "breakout": initBreakout(); break;
            case "pong": initPong(); break;
            case "wordle": initWordle(); break;
            case "hangman": initHangman(); break;
            case "typing-speed": initTyping(); break;
            case "tic-tac-toe": initTicTacToe(); break;
            case "connect-four": initConnectFour(); break;
            default: state.message = "Unknown game"; break;
        }
        render();
        canvas.focus();
    }

    function init2048() {
        state.grid = Array.from({ length: 4 }, () => [0,0,0,0]);
        state.score = 0;
        state.grid[0][0]=2; state.grid[3][3]=2;
        state.message = "2048";
    }
    function spawn2048() {
        const empty = [];
        state.grid.forEach((row,y) => row.forEach((v,x) => { if (!v) empty.push([x,y]); }));
        if (!empty.length) return;
        const [x,y] = empty[Math.floor(Math.random() * empty.length)];
        state.grid[y][x] = Math.random() < 0.9 ? 2 : 4;
    }
    function move2048(direction) {
        const before = JSON.stringify(state.grid);
        for (let i=0;i<4;i++) {
            let line = [];
            for (let j=0;j<4;j++) line.push(direction === "left" || direction === "right" ? state.grid[i][j] : state.grid[j][i]);
            if (direction === "right" || direction === "down") line.reverse();
            line = line.filter(Boolean);
            const merged = [];
            for (let j=0;j<line.length;j++) {
                if (line[j] === line[j+1]) {
                    const value = line[j] * 2;
                    merged.push(value); state.score += value; j++;
                } else merged.push(line[j]);
            }
            while (merged.length < 4) merged.push(0);
            if (direction === "right" || direction === "down") merged.reverse();
            for (let j=0;j<4;j++) {
                if (direction === "left" || direction === "right") state.grid[i][j] = merged[j];
                else state.grid[j][i] = merged[j];
            }
        }
        if (before !== JSON.stringify(state.grid)) {
            state.moves++; spawn2048();
            if (state.grid.some(row => row.includes(2048))) finish(state.score);
            else if (!canMove2048()) finish(state.score);
        }
        render();
    }
    function canMove2048() {
        for (let y=0;y<4;y++) for (let x=0;x<4;x++) {
            if (!state.grid[y][x]) return true;
            if (x<3 && state.grid[y][x]===state.grid[y][x+1]) return true;
            if (y<3 && state.grid[y][x]===state.grid[y+1][x]) return true;
        }
        return false;
    }

    function initMines() {
        const n=9, total=10;
        state.cells = Array.from({length:n},()=>Array.from({length:n},()=>({mine:false,open:false,flag:false,count:0})));
        let placed=0;
        while(placed<total){const x=Math.floor(Math.random()*n),y=Math.floor(Math.random()*n);if(!state.cells[y][x].mine){state.cells[y][x].mine=true;placed++;}}
        for(let y=0;y<n;y++) for(let x=0;x<n;x++) {
            state.cells[y][x].count=neighbors(x,y).filter(([nx,ny])=>state.cells[ny][nx].mine).length;
        }
        state.opened=0; state.flags=0; state.cursor=[4,4]; state.message="Minesweeper";
    }
    function neighbors(x,y) {
        const list=[]; for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){if(dx||dy){const nx=x+dx,ny=y+dy;if(nx>=0&&ny>=0&&nx<9&&ny<9)list.push([nx,ny]);}}
        return list;
    }
    function revealMine(x,y) {
        const cell=state.cells[y]?.[x]; if(!cell||cell.open||cell.flag||state.finished)return;
        if(cell.mine&&state.opened===0){
            cell.mine=false;
            const replacement=state.cells.flatMap((row,cy)=>row.map((candidate,cx)=>({candidate,cx,cy})))
                .find(item=>!item.candidate.mine&&(item.cx!==x||item.cy!==y));
            if(replacement)replacement.candidate.mine=true;
            for(let cy=0;cy<9;cy++)for(let cx=0;cx<9;cx++)
                state.cells[cy][cx].count=neighbors(cx,cy).filter(([nx,ny])=>state.cells[ny][nx].mine).length;
        }
        if(cell.mine){cell.open=true;finish(86_400);return;}
        const stack=[[x,y]];
        while(stack.length){const [cx,cy]=stack.pop(),c=state.cells[cy][cx];if(c.open||c.flag||c.mine)continue;c.open=true;state.opened++;if(c.count===0)neighbors(cx,cy).forEach(([nx,ny])=>stack.push([nx,ny]));}
        state.moves++;
        if(state.opened===71) finish(Math.max(1, Math.round((Date.now()-state.startedAt)/1000)));
        render();
    }
    function flagMine(x,y) {
        const cell=state.cells[y]?.[x];if(!cell||cell.open||state.finished)return;
        cell.flag=!cell.flag;state.flags+=cell.flag?1:-1;render();
    }

    function initSudoku() {
        state.grid=SUDOKU_PUZZLE.map(row=>row.split("").map(Number));
        state.solution=SUDOKU_SOLUTION.map(row=>row.split("").map(Number));
        state.selected=[0,0]; state.fixed=state.grid.map(row=>row.map(v=>v!==0));
        state.mode="easy"; state.message="Sudoku";
    }
    function sudokuInput(value) {
        const [x,y]=state.selected;
        if(state.fixed[y][x]||state.finished)return;
        state.grid[y][x]=value;
        state.moves++;
        if(state.grid.every((row,ry)=>row.every((v,rx)=>v===state.solution[ry][rx]))) finish(Math.max(1,Math.round((Date.now()-state.startedAt)/1000)),"easy");
        render();
    }

    function initMemory() {
        const values=[1,2,3,4,5,6,1,2,3,4,5,6].sort(()=>Math.random()-.5);
        state.cards=values.map(value=>({value,open:false,matched:false}));
        state.first=-1;state.locked=false;state.pairs=0;state.cursor=0;state.message="Memory Cards";
    }
    function flipMemory(index) {
        const card=state.cards[index];if(!card||card.open||card.matched||state.locked||state.finished)return;
        card.open=true;
        if(state.first<0){state.first=index;}
        else{
            state.moves++;
            const first=state.cards[state.first];
            if(first.value===card.value){first.matched=true;card.matched=true;state.pairs++;state.first=-1;if(state.pairs===6)finish(state.moves);}
            else{
                state.locked=true;
                mismatchTimeout=setTimeout(()=>{first.open=false;card.open=false;state.first=-1;state.locked=false;mismatchTimeout=0;render();},700);
            }
        }
        render();
    }

    function initFifteen() {
        state.tiles=Array.from({length:16},(_,i)=>i);
        let empty=15, previous=-1;
        for(let i=0;i<120;i++){
            const opts=[empty-4,empty+4,empty%4?empty-1:-1,empty%4<3?empty+1:-1].filter(v=>v>=0&&v<16&&v!==previous);
            const next=opts[Math.floor(Math.random()*opts.length)];
            [state.tiles[empty],state.tiles[next]]=[state.tiles[next],state.tiles[empty]];
            previous=empty;empty=next;
        }
        state.cursor=state.tiles.indexOf(0);state.message="15 Puzzle";
    }
    function moveFifteen(index) {
        const empty=state.tiles.indexOf(0);
        if([empty-4,empty+4,empty-1,empty+1].includes(index)
            && !(Math.abs(index-empty)===1 && Math.floor(index/4)!==Math.floor(empty/4))) {
            [state.tiles[empty],state.tiles[index]]=[state.tiles[index],state.tiles[empty]];
            state.cursor=state.tiles.indexOf(0);
            state.moves++;
            if(state.tiles.every((v,i)=>v===((i+1)%16)))finish(state.moves);
            render();
        }
    }

    function initLights() {
        state.grid=Array.from({length:5},()=>Array(5).fill(false));
        state.cursor=[2,2];state.message="Lights Out";
        for(let i=0;i<18;i++)toggleLight(Math.floor(Math.random()*5),Math.floor(Math.random()*5),false);
        if(state.grid.every(row=>row.every(value=>!value)))toggleLight(2,2,false);
    }
    function toggleLight(x,y,count=true) {
        [[0,0],[1,0],[-1,0],[0,1],[0,-1]].forEach(([dx,dy])=>{
            const nx=x+dx,ny=y+dy;if(nx>=0&&ny>=0&&nx<5&&ny<5)state.grid[ny][nx]=!state.grid[ny][nx];
        });
        if(count)state.moves++;
        if(count&&state.grid.every(row=>row.every(v=>!v)))finish(state.moves);
    }

    function initSnake() {
        state.snake=[[8,8],[7,8],[6,8]];state.dir=[1,0];state.nextDir=[1,0];
        state.food=[14,8];state.tick=0;state.message="Snake";
        timer=setInterval(()=>{
            if(state.paused||state.finished)return;
            state.dir=state.nextDir;
            const head=[state.snake[0][0]+state.dir[0],state.snake[0][1]+state.dir[1]];
            const eating=head[0]===state.food[0]&&head[1]===state.food[1];
            const collisionBody=eating?state.snake:state.snake.slice(0,-1);
            if(head[0]<0||head[0]>=20||head[1]<0||head[1]>=15||collisionBody.some(p=>p[0]===head[0]&&p[1]===head[1])){finish(state.score);return;}
            state.snake.unshift(head);
            if(eating){
                state.score+=10;
                if(state.snake.length===300){finish(state.score);return;}
                do { state.food=[Math.floor(Math.random()*20),Math.floor(Math.random()*15)]; }
                while(state.snake.some(p=>p[0]===state.food[0]&&p[1]===state.food[1]));
            }else state.snake.pop();
            render();
        },115);
    }
    function initTetris() {
        state.board=Array.from({length:20},()=>Array(10).fill(0));
        state.piece=null;state.pieceX=3;state.pieceY=0;state.lines=0;newPiece();
        timer=setInterval(()=>{if(!state.paused&&!state.finished){if(!movePiece(0,1))lockPiece();render();}},550);
        state.message="Tetris";
    }
    function newPiece() {
        state.piece=TETRIS_SHAPES[Math.floor(Math.random()*TETRIS_SHAPES.length)].map(row=>row.slice());
        state.pieceX=3;state.pieceY=0;
        if(!validPiece(state.piece,state.pieceX,state.pieceY))finish(state.score);
    }
    function validPiece(piece,x,y) {
        return piece.every((row,dy)=>row.every((v,dx)=>!v||(x+dx>=0&&x+dx<10&&y+dy<20&&(y+dy<0||!state.board[y+dy][x+dx]))));
    }
    function movePiece(dx,dy) {if(validPiece(state.piece,state.pieceX+dx,state.pieceY+dy)){state.pieceX+=dx;state.pieceY+=dy;return true;}return false;}
    function rotatePiece() {
        const rotated=state.piece[0].map((_,i)=>state.piece.map(row=>row[i]).reverse());
        if(validPiece(rotated,state.pieceX,state.pieceY))state.piece=rotated;
    }
    function lockPiece() {
        state.piece.forEach((row,y)=>row.forEach((v,x)=>{if(v&&state.pieceY+y>=0)state.board[state.pieceY+y][state.pieceX+x]=1;}));
        const remaining=state.board.filter(row=>row.some(v=>!v));
        const cleared=20-remaining.length;
        if(cleared){state.score+=cleared*cleared*100;state.lines+=cleared;}
        state.board=Array.from({length:cleared},()=>Array(10).fill(0)).concat(remaining);
        newPiece();
    }

    function initFlappy() {
        state.birdY=230;state.velocity=0;state.pipes=[];state.frame=0;state.score=0;state.message="Flappy";
        timer=setInterval(()=>{
            if(state.paused||state.finished)return;
            state.frame++;state.velocity+=0.42;state.birdY+=state.velocity;
            if(state.frame%95===0)state.pipes.push({x:W+20,gap:150+Math.random()*110});
            state.pipes.forEach(p=>p.x-=3);
            state.pipes=state.pipes.filter(p=>p.x>-60);
            for(const p of state.pipes){
                if(!p.passed&&p.x<115){p.passed=true;state.score++;}
                if(105+15>p.x&&105-15<p.x+55&&(state.birdY-15<p.gap-85||state.birdY+15>p.gap+85))finish(state.score);
            }
            if(state.birdY<0||state.birdY>H)finish(state.score);
            render();
        },30);
    }
    function flap() {if(state.finished)return;state.velocity=-6.5;if(state.paused)state.paused=false;}

    function initBreakout() {
        state.paddleX=300;state.ball={x:350,y:350,dx:3,dy:-3};state.bricks=[];
        for(let r=0;r<5;r++)for(let c=0;c<10;c++)state.bricks.push({x:25+c*67,y:45+r*25,alive:true});
        state.message="Breakout";
        timer=setInterval(()=>{
            if(state.paused||state.finished)return;
            const b=state.ball;b.x+=b.dx;b.y+=b.dy;
            if(b.x<8||b.x>W-8)b.dx*=-1;if(b.y<8)b.dy*=-1;
            if(b.y>H-35&&b.y<H-20&&b.x>state.paddleX&&b.x<state.paddleX+120)b.dy=-Math.abs(b.dy);
            const hit=state.bricks.find(br=>br.alive&&b.x>br.x&&b.x<br.x+58&&b.y>br.y&&b.y<br.y+16);
            if(hit){hit.alive=false;b.dy*=-1;state.score+=10;}
            if(b.y>H)finish(state.score);
            if(state.bricks.every(br=>!br.alive))finish(state.score+100);
            render();
        },20);
    }

    function initPong() {
        state.paddleY=190;state.aiY=190;state.ball={x:360,y:240,dx:4,dy:2};state.player=0;state.ai=0;state.message="Pong vs AI";
        timer=setInterval(()=>{
            if(state.paused||state.finished)return;
            const b=state.ball;b.x+=b.dx;b.y+=b.dy;
            if(b.y<8||b.y>H-8)b.dy*=-1;
            state.aiY=Math.max(0,Math.min(H-100,state.aiY+(b.y-60-state.aiY)*0.055));
            if(b.x<55&&b.x>35&&b.y>state.paddleY&&b.y<state.paddleY+100)b.dx=Math.abs(b.dx);
            if(b.x>W-55&&b.x<W-35&&b.y>state.aiY&&b.y<state.aiY+100)b.dx=-Math.abs(b.dx);
            if(b.x<0){state.ai++;resetPongBall();}
            if(b.x>W){state.player++;resetPongBall();}
            if(state.player>=5)finish(1);
            else if(state.ai>=5)finish(0);
            render();
        },20);
    }
    function resetPongBall(){state.ball={x:360,y:240,dx:state.player>state.ai?-4:4,dy:Math.random()*4-2};}

    function initWordle() {
        const words=(T.wordleWords||"CRANE,PLANT,BRICK,SOUND,MANGO").split(",").map(word=>word.trim().toUpperCase());
        state.word=words[Math.floor(Math.random()*words.length)];
        state.guesses=[];state.current="";state.message="Wordle";state.mode="classic";
    }
    function submitWordle() {
        if(state.current.length!==5||state.finished)return;
        state.guesses.push(state.current);
        if(state.current===state.word){finish(state.guesses.length,"classic");return;}
        if(state.guesses.length>=6){finish(7,"classic");return;}
        state.current="";
        render();
    }
    function initHangman() {
        const words=(T.hangmanWords||"THAIX,BRICK,PLANT,SOUND,MANGO").split(",").map(word=>word.trim().toUpperCase());
        state.word=words[Math.floor(Math.random()*words.length)];state.guessed=[];state.wrong=0;state.message="Hangman";
    }
    function guessHangman(letter) {
        if(state.finished||state.guessed.includes(letter))return;
        state.guessed.push(letter);
        if(!state.word.includes(letter))state.wrong++;
        if([...state.word].every(ch=>state.guessed.includes(ch)))finish(6-state.wrong);
        else if(state.wrong>=6)finish(0);
        render();
    }
    function initTyping() {
        state.text=T.typingPassage||"The best way to learn a new skill is to practice a little every day.";state.typed="";state.seconds=30;state.started=false;state.startedAt=0;state.correct=0;state.message="Typing Speed";
        timer=setInterval(()=>{
            if(!state.started||state.paused||state.finished)return;
            state.seconds--;
            if(state.seconds<=0)finish(calculateTypingWpm(),"30s");
            render();
        },1000);
    }
    function calculateTypingWpm() {
        const elapsedSeconds=Math.max(1,(Date.now()-state.startedAt)/1000);
        return Math.round(state.correct/5*60/elapsedSeconds);
    }
    function updateTypingStats() {
        const expected=Array.from(state.text),typed=Array.from(state.typed);
        state.correct=typed.reduce((total,char,index)=>total+(char===expected[index]?1:0),0);
    }
    function typingInput(key) {
        if(state.finished||state.seconds<=0)return;
        if(key==="Backspace"){state.typed=state.typed.slice(0,-1);updateTypingStats();render();return;}
        if(key.length!==1)return;
        if(!state.started){state.started=true;state.startedAt=Date.now();}
        if(state.typed.length<state.text.length)state.typed+=key;
        updateTypingStats();
        if(state.typed.length===state.text.length)finish(calculateTypingWpm(),"30s");
        render();
    }

    function initTicTacToe() {
        state.cells=Array(9).fill("");state.turn="X";state.cursor=4;state.message="Tic-Tac-Toe";
    }
    function ticMove(index) {
        if(state.finished||state.cells[index]||state.turn!=="X")return;
        state.cells[index]="X";
        if(checkTic("X")){finish(1);return;}
        if(state.cells.every(Boolean)){finish(0);return;}
        state.turn="O";
        const move=bestTicMove();
        if(move>=0)state.cells[move]="O";
        if(checkTic("O"))finish(0);
        else if(state.cells.every(Boolean))finish(0);
        else state.turn="X";
        render();
    }
    function checkTic(player) {
        return [[0,1,2],[3,4,5],[6,7,8],[0,3,6],[1,4,7],[2,5,8],[0,4,8],[2,4,6]].some(line=>line.every(i=>state.cells[i]===player));
    }
    function minimaxTic(depth, maximizing) {
        if(checkTic("O"))return 10-depth;
        if(checkTic("X"))return depth-10;
        if(state.cells.every(Boolean))return 0;
        let best=maximizing?-Infinity:Infinity;
        for(let i=0;i<state.cells.length;i++){
            if(state.cells[i])continue;
            state.cells[i]=maximizing?"O":"X";
            const score=minimaxTic(depth+1,!maximizing);
            state.cells[i]="";
            best=maximizing?Math.max(best,score):Math.min(best,score);
        }
        return best;
    }
    function bestTicMove() {
        let best=-Infinity,move=-1;
        for(let i=0;i<state.cells.length;i++){
            if(state.cells[i])continue;
            state.cells[i]="O";
            const score=minimaxTic(0,false);
            state.cells[i]="";
            if(score>best){best=score;move=i;}
        }
        return move;
    }

    function initConnectFour() {
        state.board=Array.from({length:6},()=>Array(7).fill(0));state.turn=1;state.cursor=3;state.message="Connect Four";
    }
    function dropConnectFour(col) {
        if(state.finished||state.turn!==1)return;
        const row=dropDisc(col,1);if(row<0)return;
        if(connectWin(1)){finish(1);return;}
        if(state.board.every(r=>r.every(Boolean))){finish(0);return;}
        state.turn=2;
        const ai=chooseConnectMove();
        dropDisc(ai,2);
        if(connectWin(2))finish(0);
        else if(state.board.every(r=>r.every(Boolean)))finish(0);
        state.turn=1;render();
    }
    function dropDisc(col,player) {
        for(let row=5;row>=0;row--)if(!state.board[row][col]){state.board[row][col]=player;return row;}
        return -1;
    }
    function connectWin(player) {
        for(let r=0;r<6;r++)for(let c=0;c<7;c++)if(state.board[r][c]===player){
            for(const [dr,dc] of [[0,1],[1,0],[1,1],[1,-1]]){
                let n=1;for(let k=1;k<4;k++){const rr=r+dr*k,cc=c+dc*k;if(rr<0||rr>=6||cc<0||cc>=7||state.board[rr][cc]!==player)break;n++;}
                if(n>=4)return true;
            }
        }
        return false;
    }
    function chooseConnectMove() {
        const valid=Array.from({length:7},(_,i)=>i).filter(c=>!state.board[0][c]);
        for(const player of [2,1])for(const c of valid){
            const r=dropDisc(c,player);const win=connectWin(player);state.board[r][c]=0;if(win)return c;
        }
        return valid.sort((a,b)=>Math.abs(a-3)-Math.abs(b-3))[0]??3;
    }

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
    function render() {
        if(disposed||!state)return;
        switch(gameId){
            case "2048": draw2048();break;case "minesweeper":drawMines();break;case "sudoku":drawSudoku();break;
            case "memory-cards":drawMemory();break;case "15-puzzle":drawFifteen();break;case "lights-out":drawLights();break;
            case "snake":drawSnake();break;case "tetris":drawTetris();break;case "flappy":drawFlappy();break;
            case "breakout":drawBreakout();break;case "pong":drawPong();break;case "wordle":drawWordle();break;
            case "hangman":drawHangman();break;case "typing-speed":drawTyping();break;case "tic-tac-toe":drawTicTacToe();break;
            case "connect-four":drawConnectFour();break;
        }
        if(state.paused&&!state.finished){rounded(220,190,280,90,16,"#0b1220");drawText(T.paused,W/2,245,28);}
        if(state.finished){rounded(190,180,340,115,16,"#0b1220",COLORS.accent);drawText(T.gameOver,W/2,220,26);drawText(`${T.score}: ${state.score}`,W/2,260,22,COLORS.good);drawText(T.restartHint,W/2,320,15,COLORS.muted);}
    }
    function draw2048(){
        frame(T.title);const size=92,gap=10,left=134,top=70;
        state.grid.forEach((row,y)=>row.forEach((v,x)=>{const px=left+x*(size+gap),py=top+y*(size+gap);rounded(px,py,size,size,10,v?tileColor(v):COLORS.panel);if(v)drawText(String(v),px+size/2,py+57,26,v>=128?"#fff":COLORS.text);}));
        drawText(`${T.score}: ${state.score}`,W/2,475,18,COLORS.muted);
    }
    function tileColor(v){return ({2:"#64748b",4:"#0e7490",8:"#0284c7",16:"#2563eb",32:"#7c3aed",64:"#9333ea",128:"#c026d3",256:"#db2777",512:"#ea580c",1024:"#ca8a04",2048:"#16a34a"})[v]||"#15803d";}
    function drawMines(){
        frame(T.title);const size=40,left=180,top=70;
        state.cells.forEach((row,y)=>row.forEach((c,x)=>{const px=left+x*size,py=top+y*size;rounded(px+1,py+1,size-3,size-3,c.open?"#cbd5e1":COLORS.tile, "#64748b");if(c.open){if(c.mine)drawText("*",px+20,py+28,23,COLORS.bad);else if(c.count)drawText(String(c.count),px+20,py+27,18,["", "#2563eb","#16a34a","#dc2626","#7c3aed"][c.count]||"#0f172a");}else if(c.flag)drawText("F",px+20,py+27,18,COLORS.warn);if(state.cursor[0]===x&&state.cursor[1]===y){ctx.strokeStyle=COLORS.accent;ctx.lineWidth=3;ctx.strokeRect(px+3,py+3,size-6,size-6);ctx.lineWidth=1;}}));
        drawText(`${T.flags}: ${state.flags}/10`,W/2,465,17,COLORS.muted);
        rounded(575,442,115,28,5,state.flagMode?"#854d0e":COLORS.tile);
        drawText(T.flags,632,461,13);
    }
    function drawSudoku(){
        frame(T.title);const size=42,left=171,top=55;
        for(let y=0;y<9;y++)for(let x=0;x<9;x++){const px=left+x*size,py=top+y*size;ctx.fillStyle=state.selected[0]===x&&state.selected[1]===y?"#075985":(Math.floor(x/3)+Math.floor(y/3))%2?"#1e293b":"#273449";ctx.fillRect(px,py,size,size);ctx.strokeStyle="#64748b";ctx.strokeRect(px,py,size,size);if(state.grid[y][x])drawText(String(state.grid[y][x]),px+21,py+28,19,state.fixed[y][x]?"#f8fafc":"#7dd3fc");}
        ctx.strokeStyle="#e2e8f0";ctx.lineWidth=3;for(let i=0;i<=9;i+=3){ctx.strokeRect(left+i*size,top,1,9*size);ctx.strokeRect(left,top+i*size,9*size,1);}ctx.lineWidth=1;
        for(let i=1;i<=9;i++){const x=104+(i-1)*57;rounded(x,440,50,30,5,COLORS.tile);drawText(String(i),x+25,462,16);}
        rounded(615,440,50,30,5,COLORS.tile);drawText("DEL",640,462,12);
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
        [["<",245],["ROT",315],[">",385],["DROP",455]].forEach(([label,x])=>{rounded(x,438,62,30,5,COLORS.tile);drawText(label,x+31,459,13);});
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
        for(let y=0;y<6;y++)for(let x=0;x<5;x++){
            const px=left+x*(size+gap),py=top+y*(size+gap),guess=state.guesses[y]||"",ch=guess[x]|| (y===state.guesses.length?state.current[x]||"":"");
            let fill=COLORS.panel;
            if(guess){const status=getWordleStatuses(guess)[x];fill=status==="correct"?"#15803d":status==="present"?"#a16207":"#475569";}
            rounded(px,py,size,size,6,fill,COLORS.line);if(ch)drawText(ch,px+size/2,py+36,23);
        }
        const rows=["QWERTYUIOP","ASDFGHJKL","ZXCVBNM"];
        rows.forEach((row,ri)=>[...row].forEach((letter,i)=>{const keyW=34,x=185+i*36+(ri===1?18:ri===2?54:0),y=345+ri*38;rounded(x,y,keyW,30,4,COLORS.tile);drawText(letter,x+keyW/2,y+21,14);}));
        rounded(545,421,65,28,4,COLORS.tile);drawText("DEL",577,440,12);
        rounded(615,421,65,28,4,COLORS.tile);drawText("ENTER",647,440,11);
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
        const words=state.text.split(" ");let y=145,x=75;ctx.fillStyle=COLORS.muted;
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

    function point(event) {
        const rect=canvas.getBoundingClientRect();
        return [(event.clientX-rect.left)*W/rect.width,(event.clientY-rect.top)*H/rect.height];
    }
    function onPointerDown(event) {
        event.preventDefault();canvas.focus();const [x,y]=point(event);pointerStart=[x,y];
        if(state.paused||state.finished)return;
        if(gameId==="snake"&&x>605&&y>325){if(y<365)state.nextDir=[0,-1];else if(y>405)state.nextDir=[0,1];else if(x<645)state.nextDir=[-1,0];else state.nextDir=[1,0];return;}
        if(gameId==="tetris"&&y>430){if(x<310)movePiece(-1,0);else if(x<380)rotatePiece();else if(x<450)movePiece(1,0);else{while(movePiece(0,1)){}lockPiece();}render();return;}
        if(gameId==="breakout"){state.paddleX=Math.max(0,Math.min(W-120,x-60));render();return;}
        if(gameId==="pong"){state.paddleY=Math.max(0,Math.min(H-100,y-50));return;}
        if(gameId==="sudoku"&&y>438){if(x>=615)sudokuInput(0);else{const value=Math.floor((x-104)/57)+1;if(value>=1&&value<=9)sudokuInput(value);}return;}
        if(gameId==="wordle"&&y>=345){const row=y<380?0:y<418?1:2;const keyRows=["QWERTYUIOP","ASDFGHJKL","ZXCVBNM"];const offset=row===0?185:row===1?203:239;const index=Math.floor((x-offset)/36);const key=keyRows[row][index];if(key&&index>=0&&index<keyRows[row].length){state.current=(state.current+key).slice(0,5);render();}else if(y>=421&&x>=545&&x<610){state.current=state.current.slice(0,-1);render();}else if(y>=421&&x>=615){submitWordle();}return;}
        if(gameId==="hangman"&&y>=345){const row=y<380?0:y<418?1:2;const keyRows=["QWERTYUIOP","ASDFGHJKL","ZXCVBNM"];const offset=row===0?185:row===1?203:239;const index=Math.floor((x-offset)/36);const key=keyRows[row][index];if(key&&index>=0&&index<keyRows[row].length)guessHangman(key);return;}
        if(gameId==="minesweeper"){
            if(x>570&&y>435){state.flagMode=!state.flagMode;render();return;}
            const cx=Math.floor((x-180)/40),cy=Math.floor((y-70)/40);
            if(cx>=0&&cy>=0&&cx<9&&cy<9)state.cursor=[cx,cy];
            if(event.button===2||state.flagMode)flagMine(cx,cy);else revealMine(cx,cy);
        } else if(gameId==="memory-cards"){
            const cx=Math.floor((x-150)/116),cy=Math.floor((y-70)/116);if(cx>=0&&cy>=0&&cx<4&&cy<3){state.cursor=cy*4+cx;flipMemory(state.cursor);}
        } else if(gameId==="15-puzzle"){
            const cx=Math.floor((x-160)/104),cy=Math.floor((y-65)/104);if(cx>=0&&cy>=0&&cx<4&&cy<4){state.cursor=cy*4+cx;moveFifteen(state.cursor);}
        } else if(gameId==="lights-out"){
            const cx=Math.floor((x-180)/78),cy=Math.floor((y-65)/78);if(cx>=0&&cy>=0&&cx<5&&cy<5){state.cursor=[cx,cy];toggleLight(cx,cy);}
            render();
        } else if(gameId==="sudoku"){
            const cx=Math.floor((x-171)/42),cy=Math.floor((y-55)/42);if(cx>=0&&cy>=0&&cx<9&&cy<9)state.selected=[cx,cy];render();
        } else if(gameId==="flappy")flap();
        else if(gameId==="connect-four"){const col=Math.round((x-158)/67);if(col>=0&&col<7){state.cursor=col;dropConnectFour(col);}}
        else if(gameId==="tic-tac-toe"){const col=Math.floor((x-192)/112),row=Math.floor((y-65)/112);if(col>=0&&col<3&&row>=0&&row<3){state.cursor=row*3+col;ticMove(state.cursor);}}
    }
    function onPointerUp(event) {
        if(gameId==="2048"&&pointerStart&&!state.paused&&!state.finished){
            const [sx,sy]=pointerStart,[x,y]=point(event),dx=x-sx,dy=y-sy;
            if(Math.max(Math.abs(dx),Math.abs(dy))>25)move2048(Math.abs(dx)>Math.abs(dy)?(dx>0?"right":"left"):(dy>0?"down":"up"));
        }
        pointerStart=null;
    }
    function onTypingFieldInput() {
        if(!typingField)return;
        if(state.paused||state.finished){typingField.value=state.typed;return;}
        state.typed=Array.from(typingField.value).slice(0,Array.from(state.text).length).join("");
        typingField.value=state.typed;
        if(state.typed.length&&!state.started){state.started=true;state.startedAt=Date.now();}
        updateTypingStats();
        if(Array.from(state.typed).length===Array.from(state.text).length)finish(calculateTypingWpm(),"30s");
        render();
    }

    function onKeyDown(event) {
        if(state.paused||state.finished)return;
        const key=event.key.toLowerCase();
        if(gameId==="typing-speed"){event.preventDefault();typingInput(event.key);if(typeof typingField?.addEventListener === "function")typingField.value=state.typed;return;}
        if(["arrowup","arrowdown","arrowleft","arrowright"," "].includes(key))event.preventDefault();
        switch(gameId){
            case "2048": if(key.startsWith("arrow"))move2048(key.slice(5));break;
            case "minesweeper": if(key==="f")flagMine(state.cursor[0],state.cursor[1]);else if(key==="enter"||key===" "){if(state.flagMode)flagMine(state.cursor[0],state.cursor[1]);else revealMine(state.cursor[0],state.cursor[1]);}else if(key.startsWith("arrow")){const [x,y]=state.cursor;state.cursor=[Math.max(0,Math.min(8,x+(key==="arrowright"?1:key==="arrowleft"?-1:0))),Math.max(0,Math.min(8,y+(key==="arrowdown"?1:key==="arrowup"?-1:0)))];render();}break;
            case "memory-cards": if(key.startsWith("arrow")){let row=Math.floor(state.cursor/4),col=state.cursor%4;if(key==="arrowright"&&col<3)col++;if(key==="arrowleft"&&col>0)col--;if(key==="arrowdown"&&row<2)row++;if(key==="arrowup"&&row>0)row--;state.cursor=row*4+col;render();}else if(key==="enter"||key===" ")flipMemory(state.cursor);break;
            case "15-puzzle": if(key.startsWith("arrow")){const empty=state.tiles.indexOf(0);const tile=key==="arrowleft"?(empty%4<3?empty+1:-1):key==="arrowright"?(empty%4>0?empty-1:-1):key==="arrowup"?(empty<12?empty+4:-1):(empty>=4?empty-4:-1);if(tile>=0)moveFifteen(tile);}break;
            case "lights-out": if(key.startsWith("arrow")){const [x,y]=state.cursor;state.cursor=[Math.max(0,Math.min(4,x+(key==="arrowright"?1:key==="arrowleft"?-1:0))),Math.max(0,Math.min(4,y+(key==="arrowdown"?1:key==="arrowup"?-1:0)))];render();}else if(key==="enter"||key===" ")toggleLight(state.cursor[0],state.cursor[1]);break;
            case "tic-tac-toe": if(key.startsWith("arrow")){let row=Math.floor(state.cursor/3),col=state.cursor%3;if(key==="arrowright"&&col<2)col++;if(key==="arrowleft"&&col>0)col--;if(key==="arrowdown"&&row<2)row++;if(key==="arrowup"&&row>0)row--;state.cursor=row*3+col;render();}else if(key==="enter"||key===" ")ticMove(state.cursor);break;
            case "connect-four": if(key==="arrowleft")state.cursor=Math.max(0,state.cursor-1);else if(key==="arrowright")state.cursor=Math.min(6,state.cursor+1);else if(key==="enter"||key===" ")dropConnectFour(state.cursor);render();break;
            case "sudoku": if(key.startsWith("arrow")){const [x,y]=state.selected;state.selected=[Math.max(0,Math.min(8,x+(key==="arrowright"?1:key==="arrowleft"?-1:0))),Math.max(0,Math.min(8,y+(key==="arrowdown"?1:key==="arrowup"?-1:0)))];render();}else if(/^[1-9]$/.test(key))sudokuInput(Number(key));else if(key==="backspace"||key==="delete"){const [x,y]=state.selected;if(!state.fixed[y][x]){state.grid[y][x]=0;render();}}break;
            case "snake": if(key==="arrowup"||key==="w"){if(state.dir[1]!==1)state.nextDir=[0,-1];}if(key==="arrowdown"||key==="s"){if(state.dir[1]!==-1)state.nextDir=[0,1];}if(key==="arrowleft"||key==="a"){if(state.dir[0]!==1)state.nextDir=[-1,0];}if(key==="arrowright"||key==="d"){if(state.dir[0]!==-1)state.nextDir=[1,0];}break;
            case "tetris": if(key==="arrowleft"||key==="a")movePiece(-1,0);if(key==="arrowright"||key==="d")movePiece(1,0);if(key==="arrowup"||key==="w")rotatePiece();if(key==="arrowdown")movePiece(0,1);if(key===" "){while(movePiece(0,1)){}lockPiece();}render();break;
            case "flappy": if(key===" "||key==="arrowup")flap();break;
            case "breakout": if(key==="arrowleft"||key==="a")state.paddleX=Math.max(0,state.paddleX-35);if(key==="arrowright"||key==="d")state.paddleX=Math.min(W-120,state.paddleX+35);render();break;
            case "pong": if(key==="arrowup"||key==="w")state.paddleY=Math.max(0,state.paddleY-35);if(key==="arrowdown"||key==="s")state.paddleY=Math.min(H-100,state.paddleY+35);break;
            case "wordle": if(key==="enter")submitWordle();else if(key==="backspace")state.current=state.current.slice(0,-1);else if(/^[a-z]$/.test(key)&&state.current.length<5)state.current+=key.toUpperCase();render();break;
            case "hangman": if(/^[a-z]$/.test(key))guessHangman(key.toUpperCase());break;
            case "typing-speed": if(key==="backspace")typingInput("Backspace");else if(event.key.length===1)typingInput(event.key);break;
        }
    }
    function onContextMenu(event){if(gameId==="minesweeper")event.preventDefault();}
    canvas.addEventListener("pointerdown",onPointerDown);
    canvas.addEventListener("pointerup",onPointerUp);
    canvas.addEventListener("keydown",onKeyDown);
    if(typeof typingField?.addEventListener === "function")typingField.addEventListener("input",onTypingFieldInput);
    canvas.addEventListener("contextmenu",onContextMenu);
    reset();

    return {
        restart: reset,
        pause(value) {
            if(value&&!state.paused)state.pausedAt=Date.now();
            else if(!value&&state.paused&&state.pausedAt){
                if(state.startedAt)state.startedAt+=Date.now()-state.pausedAt;
                state.pausedAt=0;
            }
            state.paused=value;render();
        },
        dispose() {
            disposed=true;clearInterval(timer);clearTimeout(mismatchTimeout);mismatchTimeout=0;cancelAnimationFrame(animationFrame);
            canvas.removeEventListener("pointerdown",onPointerDown);
            canvas.removeEventListener("pointerup",onPointerUp);
            canvas.removeEventListener("keydown",onKeyDown);
            if(typeof typingField?.removeEventListener === "function")typingField.removeEventListener("input",onTypingFieldInput);
            canvas.removeEventListener("contextmenu",onContextMenu);
        }
    };
}
