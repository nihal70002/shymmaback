const fs = require('fs');

const filePath = 'c:\\Users\\Owner\\source\\repos\\shymmafront\\src\\pages\\public\\ProductDetails.jsx';
let content = fs.readFileSync(filePath, 'utf8');

// Fix the broken className with line break
const brokenPattern = /className="min-w-\[3rem\] px-4 py-2 rounded-lg border-2 font-bold text-xs sm:text-sm flex items-center justify-center transition text-center\s+border-gray-300 text-gray-900 hover:border-teal-400"/g;
const fixedPattern = 'className="min-w-[3rem] px-4 py-2 rounded-lg border-2 font-bold text-xs sm:text-sm flex items-center justify-center transition text-center border-gray-300 text-gray-900 hover:border-teal-400"';

content = content.replace(brokenPattern, fixedPattern);

fs.writeFileSync(filePath, content, 'utf8');
console.log('Fixed className line break');
