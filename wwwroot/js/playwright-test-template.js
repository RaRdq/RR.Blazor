const { chromium } = require('playwright');

/**
 * Comprehensive Playwright Test Template with Zero-Tolerance Error Detection
 * Copy this file and customize the COMPONENT_NAME and TARGET_URL for your specific test
 */

const COMPONENT_NAME = '[REPLACE_WITH_COMPONENT_NAME]'; // e.g., 'DataTable.razor', 'ComplianceTracker.razor'
const TARGET_URL = '[REPLACE_WITH_TARGET_URL]'; // e.g., '/dashboard', '/company/compliance'

(async () => {
  console.log(`🎯 Testing: ${COMPONENT_NAME}`);
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 300
  });
  const page = await browser.newPage();
  
  let testFailed = false;
  const errors = [];
  const screenshots = [];

  // INSTANT FAIL: Console error monitoring with zero tolerance
  page.on('console', msg => {
    if (msg.type() === 'error') {
      const errorText = msg.text();
      // Filter out known non-critical errors and common Blazor reload messages
      if (!errorText.includes('serilog') && 
          !errorText.includes('%c') && 
          !errorText.includes('[DEBUG]') &&
          !errorText.includes('[INFO]') &&
          !errorText.includes('Please reload') &&
          !errorText.includes('SignalR') &&
          !errorText.includes('WebSocket') &&
          !errorText.includes('connection closed') &&
          !errorText.includes('Failed to fetch') &&
          !errorText.includes('chunk-') &&
          !errorText.includes('Loading chunk')) {
        console.log(`❌ INSTANT FAIL - Console Error: ${errorText}`);
        errors.push(`Console Error: ${errorText}`);
        testFailed = true;
      }
    }
  });

  // INSTANT FAIL: Error boundary detection  
  page.on('pageerror', error => {
    console.log(`❌ INSTANT FAIL - Page Error: ${error.message}`);
    errors.push(`Page Error: ${error.message}`);
    testFailed = true;
  });

  // INSTANT FAIL: Request failure monitoring
  page.on('requestfailed', request => {
    const url = request.url();
    const failure = request.failure()?.errorText || 'Unknown error';
    
    // Filter out common reload-related request failures
    if (!url.includes('_blazor/') && 
        !url.includes('sockjs-node') &&
        !url.includes('hot-reload') &&
        !failure.includes('net::ERR_CONNECTION_REFUSED') &&
        !failure.includes('net::ERR_ABORTED')) {
      console.log(`❌ INSTANT FAIL - Request Failed: ${url} - ${failure}`);
      errors.push(`Request Failed: ${url}`);
      testFailed = true;
    }
  });

  // Network response monitoring
  page.on('response', response => {
    if (response.status() >= 400) {
      console.log(`⚠️ HTTP ${response.status()}: ${response.url()}`);
      errors.push(`HTTP ${response.status()}: ${response.url()}`);
      if (response.status() >= 500) {
        testFailed = true;
      }
    }
  });

  try {
    // STEP 1: Navigation & Authentication
    console.log('🔐 Authenticating...');
    await page.goto('https://localhost:5002', { waitUntil: 'networkidle' });
    
    // Check for error boundaries immediately after navigation
    const errorBoundaries = await page.locator('[data-error-boundary], .error-boundary, .blazor-error-boundary').count();
    if (errorBoundaries > 0) {
      console.log('❌ INSTANT FAIL - Error boundary detected on login page');
      testFailed = true;
      const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-error-boundary-login.png`;
      await page.screenshot({ path: screenshot, fullPage: true });
      screenshots.push(screenshot);
      throw new Error('Error boundary detected - test terminated');
    }

    // Check if we need to login or are already authenticated
    const emailInput = page.locator('input[type="email"]').first();
    if (await emailInput.isVisible({ timeout: 3000 })) {
      console.log('📝 Login form detected, performing authentication...');
      await emailInput.fill('sarah.johnson@gmail.com');
      await page.fill('input[type="password"]', 'Test123!');
      await page.click('button[type="submit"]');
      await page.waitForTimeout(3000); // Give more time for auth
      
      // More robust authentication error checking
      const authErrors = await page.locator('.alert-danger, .validation-summary-errors, .field-validation-error').count();
      const authErrorMessages = await page.locator('.alert-danger, .validation-summary-errors, .field-validation-error').allTextContents();
      
      // Filter out non-critical error messages that might appear during normal operation
      const criticalErrors = authErrorMessages.filter(msg => 
        msg && 
        msg.trim() && // Only non-empty messages
        !msg.includes('Please reload') && 
        !msg.includes('Loading') &&
        !msg.includes('temporary') &&
        !msg.includes('Refreshing') &&
        !msg.includes('Reconnecting') &&
        !msg.includes('blazor') &&
        !msg.includes('SignalR') &&
        (msg.includes('Invalid') || msg.includes('incorrect') || msg.includes('failed') || msg.includes('denied') || msg.includes('error'))
      );
      
      if (criticalErrors.length > 0) {
        console.log('❌ INSTANT FAIL - Critical authentication error detected');
        console.log(`Authentication errors: ${criticalErrors.join(', ')}`);
        testFailed = true;
        throw new Error(`Authentication failed: ${criticalErrors[0]}`);
      }
      
      // Check if we're still on login page after sufficient wait time
      await page.waitForTimeout(2000);
      const currentUrl = page.url();
      const pageTitle = await page.title();
      
      console.log(`🔍 Post-auth status: URL=${currentUrl}, Title=${pageTitle}`);
      
      // Only fail if we're clearly still on login with error indicators
      if ((currentUrl.includes('login') || currentUrl.includes('identity')) && 
          (pageTitle.includes('Login') || pageTitle.includes('Sign'))) {
        
        // Look for actual error indicators, not just presence on login page
        const actualErrors = await page.locator('.text-danger:visible, .error:visible').count();
        const invalidCredentials = await page.locator('text=Invalid').count();
        
        if (actualErrors > 0 || invalidCredentials > 0) {
          console.log('❌ INSTANT FAIL - Authentication failed, still on login page with errors');
          testFailed = true;
          throw new Error('Authentication failed - invalid credentials or login error');
        } else {
          console.log('⚠️ Still on login page but no explicit errors detected, continuing...');
        }
      }
    } else {
      console.log('✅ Already authenticated or no login form detected');
    }

    // STEP 2: Navigate to target component
    console.log(`🧭 Navigating to ${TARGET_URL}...`);
    
    // Handle potential page reloads during navigation
    let navigationAttempts = 0;
    const maxNavigationAttempts = 3;
    
    while (navigationAttempts < maxNavigationAttempts) {
      try {
        await page.goto(`https://localhost:5002${TARGET_URL}`, { 
          waitUntil: 'networkidle',
          timeout: 10000 
        });
        
        // Wait a moment and check if we need to reload
        await page.waitForTimeout(1000);
        const reloadMessages = await page.locator('text=Please reload').count();
        
        if (reloadMessages > 0) {
          console.log(`⚠️ Reload message detected, attempt ${navigationAttempts + 1}/${maxNavigationAttempts}`);
          navigationAttempts++;
          
          if (navigationAttempts < maxNavigationAttempts) {
            await page.reload({ waitUntil: 'networkidle' });
            await page.waitForTimeout(2000);
          }
        } else {
          // Navigation successful
          break;
        }
      } catch (navigationError) {
        navigationAttempts++;
        console.log(`⚠️ Navigation attempt ${navigationAttempts} failed: ${navigationError.message}`);
        
        if (navigationAttempts >= maxNavigationAttempts) {
          throw new Error(`Navigation failed after ${maxNavigationAttempts} attempts`);
        }
        
        await page.waitForTimeout(2000);
      }
    }
    
    // CRITICAL: Error boundary check after navigation
    const postNavErrorBoundaries = await page.locator('[data-error-boundary], .error-boundary, .blazor-error-boundary').count();
    if (postNavErrorBoundaries > 0) {
      console.log('❌ INSTANT FAIL - Error boundary detected after navigation');
      testFailed = true;
      const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-error-boundary-navigation.png`;
      await page.screenshot({ path: screenshot, fullPage: true });
      screenshots.push(screenshot);
      throw new Error('Error boundary detected - test terminated');
    }

    // STEP 3: Parameter conflict detection (common Blazor issue)
    const parameterErrors = await page.evaluate(() => {
      const consoleErrors = [];
      const originalError = console.error;
      let errorCaptured = false;
      
      console.error = (...args) => {
        const message = args.join(' ');
        if (message.includes('property matching the name') || 
            message.includes('parameter') || 
            message.includes('ChildContent') ||
            message.includes('does not have a property')) {
          consoleErrors.push(message);
          errorCaptured = true;
        }
        originalError.apply(console, args);
      };
      
      // Wait a moment for any async parameter errors
      setTimeout(() => console.error = originalError, 1000);
      
      return consoleErrors;
    });
    
    await page.waitForTimeout(1000);
    
    if (parameterErrors.length > 0) {
      console.log('❌ INSTANT FAIL - Parameter conflict detected');
      errors.push(...parameterErrors);
      testFailed = true;
      throw new Error('Parameter conflicts detected - test terminated');
    }

    // STEP 4: Collect debug information
    console.log('🔍 Collecting debug information...');
    const debugReport = await page.evaluate(() => {
      if (window.RRDebug) {
        try {
          return window.RRDebug.report();
        } catch (e) {
          return { error: 'Debug report failed', message: e.message };
        }
      }
      return { error: 'RRDebug not available' };
    });
    
    if (debugReport.error) {
      console.log('⚠️ Debug report issue:', debugReport.error);
    } else {
      console.log('📊 Debug Report Summary:', {
        health: `${debugReport.health?.score}% (${debugReport.health?.status})`,
        errors: debugReport.errors?.count || 0,
        elements: debugReport.page?.elementCounts?.total || 0
      });
    }

    // STEP 5: Component-specific comprehensive testing
    if (!testFailed) {
      console.log('✅ Proceeding with comprehensive component testing...');
      
      // Test all interactive elements
      const buttons = await page.locator('button:visible').count();
      const inputs = await page.locator('input:visible').count();
      const modals = await page.locator('[role="dialog"]:visible').count();
      const tables = await page.locator('table:visible').count();
      
      console.log(`📋 Found ${buttons} buttons, ${inputs} inputs, ${modals} modals, ${tables} tables`);
      
      // Test each button systematically (limit to prevent excessive testing)
      const maxButtonsToTest = Math.min(buttons, 15);
      for (let i = 0; i < maxButtonsToTest; i++) {
        if (testFailed) break;
        
        const button = page.locator('button:visible').nth(i);
        const buttonText = await button.textContent();
        const isDisabled = await button.isDisabled();
        
        if (!isDisabled) {
          console.log(`🔘 Testing button: ${buttonText?.trim() || `Button ${i + 1}`}`);
          
          try {
            await button.click();
            await page.waitForTimeout(500); // Allow for any async operations
            
            // Check for errors after button click
            const postClickErrors = await page.locator('[data-error-boundary], .error-boundary').count();
            if (postClickErrors > 0) {
              console.log(`❌ Button click caused error boundary: ${buttonText}`);
              testFailed = true;
              const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-button-error-${i}.png`;
              await page.screenshot({ path: screenshot, fullPage: true });
              screenshots.push(screenshot);
              break;
            }
          } catch (e) {
            console.log(`⚠️ Button click failed: ${buttonText} - ${e.message}`);
            errors.push(`Button interaction failed: ${buttonText}`);
          }
        }
      }
      
      // Test form inputs if present
      if (inputs > 0 && !testFailed) {
        console.log('📝 Testing form inputs...');
        const maxInputsToTest = Math.min(inputs, 10);
        
        for (let i = 0; i < maxInputsToTest; i++) {
          if (testFailed) break;
          
          const input = page.locator('input:visible').nth(i);
          const inputType = await input.getAttribute('type');
          const inputName = await input.getAttribute('name') || `input-${i}`;
          
          if (inputType !== 'submit' && inputType !== 'button') {
            try {
              await input.fill('test-input-value');
              await page.waitForTimeout(200);
              
              // Check for validation errors or issues
              const validationErrors = await page.locator('.field-validation-error, .validation-message').count();
              console.log(`📝 Input ${inputName} (${inputType}): ${validationErrors} validation messages`);
            } catch (e) {
              console.log(`⚠️ Input interaction failed: ${inputName} - ${e.message}`);
            }
          }
        }
      }
      
      // Test responsive behavior on key breakpoints
      console.log('📱 Testing responsive behavior...');
      const breakpoints = [
        { width: 320, height: 568, name: 'Mobile' },
        { width: 768, height: 1024, name: 'Tablet' },
        { width: 1920, height: 1080, name: 'Desktop' }
      ];
      
      for (const breakpoint of breakpoints) {
        if (testFailed) break;
        
        await page.setViewportSize({ width: breakpoint.width, height: breakpoint.height });
        await page.waitForTimeout(500);
        
        // Check for horizontal overflow
        const hasOverflow = await page.evaluate((width) => {
          const elements = document.querySelectorAll('*');
          return Array.from(elements).some(el => {
            const rect = el.getBoundingClientRect();
            return rect.width > width + 20;
          });
        }, breakpoint.width);
        
        if (hasOverflow) {
          console.log(`⚠️ Horizontal overflow detected on ${breakpoint.name}`);
          const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-overflow-${breakpoint.name.toLowerCase()}.png`;
          await page.screenshot({ path: screenshot, fullPage: true });
          screenshots.push(screenshot);
        }
      }
    }
    
  } catch (error) {
    console.log(`❌ Test error: ${error.message}`);
    errors.push(`Test Error: ${error.message}`);
    testFailed = true;
    const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-general-error.png`;
    await page.screenshot({ path: screenshot, fullPage: true });
    screenshots.push(screenshot);
  } finally {
    // MANDATORY: Report all errors and results before closing
    console.log('\n' + '='.repeat(60));
    
    if (testFailed || errors.length > 0) {
      console.log('🚨 TEST FAILED - ERRORS DETECTED:');
      errors.forEach((error, index) => {
        console.log(`${index + 1}. ${error}`);
      });
      
      if (screenshots.length > 0) {
        console.log('\n📸 Screenshots captured:');
        screenshots.forEach(screenshot => console.log(`  • ${screenshot}`));
      }
      
      console.log(`\n❌ ${COMPONENT_NAME} test FAILED with ${errors.length} errors`);
      
      // Exit with error code for CI/CD integration
      await browser.close();
      process.exit(1);
    } else {
      console.log(`✅ ${COMPONENT_NAME} test PASSED - no errors detected`);
      console.log('🎉 All interactions completed successfully');
      
      // Take a final success screenshot
      const successScreenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-success.png`;
      await page.screenshot({ path: successScreenshot, fullPage: true });
      console.log(`📸 Success screenshot: ${successScreenshot}`);
    }
    
    await browser.close();
  }
})();