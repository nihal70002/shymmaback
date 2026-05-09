const fs = require('fs');

// Fix: Landing.jsx - Make Featured Product cards same height
let landing = fs.readFileSync('src/pages/public/Landing.jsx', 'utf8');

landing = landing.replace(
  'className="group block bg-white rounded-2xl border border-gray-200 shadow-md hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden"',
  'className="group flex flex-col w-full bg-white rounded-2xl border border-gray-200 shadow-md hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden"'
);

landing = landing.replace(
  `className="p-4">
                        <p className="font-semibold text-sm line-clamp-2">`,
  `className="flex-1 p-4">
                        <p className="font-semibold text-sm line-clamp-2 min-h-[40px]">`
);

fs.writeFileSync('src/pages/public/Landing.jsx', landing);
console.log('Fixed Landing.jsx card heights');
