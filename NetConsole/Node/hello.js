process.stdin.setEncoding('utf8');

process.stdin.on("data", rawLine => {
    const line = rawLine.trim();

    const firstMsg = "Aqui é o ";
    if (line.indexOf(firstMsg) === 0) {
        const whoIs = line.substring(firstMsg.length);
        console.log(`Oi ${whoIs}. Aqui é o Node.`);
        return;
    }

    if (line.indexOf("SUM") === 0) {
        let [num1, num2] = line.substring(3).split(",");
        console.log(parseInt(num1) + parseInt(num2));
        return;
    }

    console.log(line);
});