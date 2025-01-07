// import { newSpecPage } from '@stencil/core/testing';
// import { CpsGlobalNav } from './cps-global-nav';

// describe('cps-global-nav', () => {
//   it('renders', async () => {
//     const { root } = await newSpecPage({
//       components: [CpsGlobalNav],
//       html: '<cps-global-nav></cps-global-nav>',
//     });
//     expect(root).toEqualHtml(`
//       <cps-global-nav>
//         <mock:shadow-root>
//           <div>
//             Hello, World! I'm
//           </div>
//         </mock:shadow-root>
//       </cps-global-nav>
//     `);
//   });

//   it('renders with values', async () => {
//     const { root } = await newSpecPage({
//       components: [CpsGlobalNav],
//       html: `<cps-global-nav first="Stencil" middle="'Don't call me a framework'" last="JS"></cps-global-nav>`,
//     });
//     expect(root).toEqualHtml(`
//       <cps-global-nav first="Stencil" middle="'Don't call me a framework'" last="JS">
//         <mock:shadow-root>
//           <div>
//             Hello, World! I'm Stencil 'Don't call me a framework' JS
//           </div>
//         </mock:shadow-root>
//       </cps-global-nav>
//     `);
//   });
// });
